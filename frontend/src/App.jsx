import React, { useState, useEffect } from "react";
import "./App.css";
import Tesseract from "tesseract.js";

import { GlobalWorkerOptions, getDocument } from "pdfjs-dist";
import pdfWorker from "pdfjs-dist/build/pdf.worker?url";

const App = () => {
  const [resumeFileName, setResumeFileName] = useState("No file uploaded");
  const [apiKey, setApiKey] = useState("");
  const [isUploading, setIsUploading] = useState(false);
  const [isApiKeySaved, setIsApiKeySaved] = useState(false);
  const [isResumeSaved, setIsResumeSaved] = useState(false);

  useEffect(() => {
    const checkApiKey = async () => {
      try {
        const response = await fetch(
          "http://localhost:5182/api/ApiKey/IsEmpty"
        );
        if (response.ok) {
          const result = await response.json();
          setIsApiKeySaved(result.isSaved);
        }
      } catch (error) {
        console.error("Error checking API key:", error);
      }
    };

    checkApiKey();
  }, []);

  useEffect(() => {
    const checkResume = async () => {
      try {
        const response = await fetch(
          "http://localhost:5182/api/Resume/IsResumeRecordEmpty"
        );
        if (response.ok) {
          const result = await response.json();
          setIsResumeSaved(result.isSaved);
        }
      } catch (error) {
        console.error("Error checking Resume:", error);
      }
    };
    checkResume();
  }, []);

  // To handle screenshotted resumes
  const extractTextFromPDF = async (file) => {
    try {
      GlobalWorkerOptions.workerSrc = pdfWorker;

      const arrayBuffer = await file.arrayBuffer();
      const pdf = await getDocument({ data: arrayBuffer }).promise;
      let fullText = "";

      for (let pageNum = 1; pageNum <= pdf.numPages; pageNum++) {
        const page = await pdf.getPage(pageNum);
        const textContent = await page.getTextContent();
        const pageText = textContent.items
          .map((item) => item.str || "")
          .join(" ")
          .replace(/\s+/g, " ")
          .trim();

        if (pageText.length > 0) {
          fullText += pageText + "\n";
        } else {
          // OCR fallback (rare if PDF is from Word)
          const canvas = document.createElement("canvas");
          const viewport = page.getViewport({ scale: 2 });
          canvas.width = viewport.width;
          canvas.height = viewport.height;
          const ctx = canvas.getContext("2d");

          await page.render({ canvasContext: ctx, viewport }).promise;
          const ocrResult = await Tesseract.recognize(canvas, "eng");
          fullText += ocrResult.data.text + "\n";
        }
      }

      return fullText.trim();
    } catch (error) {
      console.error("Error extracting PDF text:", error);
      throw new Error("Failed to extract text from PDF.");
    }
  };

  const handleResumeUpload = async (event) => {
    if (event.target.files.length > 0) {
      const file = event.target.files[0];
      setResumeFileName(file.name);
      setIsUploading(true);

      try {
        let fileText;

        // Check file type and extract text accordingly
        if (
          file.type === "application/pdf" ||
          file.name.toLowerCase().endsWith(".pdf")
        ) {
          console.log("Processing PDF file...");
          fileText = await extractTextFromPDF(file);
        } else if (
          file.type === "text/plain" ||
          file.name.toLowerCase().endsWith(".txt")
        ) {
          fileText = await file.text();
        } else {
          alert("Unsupported file type. Please upload a PDF or TXT file.");
          setIsUploading(false);
          return;
        }

        // Check if we got any text
        if (!fileText || fileText.trim().length === 0) {
          alert(
            "No text could be extracted from the file. Please make sure it contains readable text."
          );
          setIsUploading(false);
          return;
        }

        console.log(
          "Extracted text preview:",
          fileText.substring(0, 200) + "..."
        );

        const response = await fetch(
          "http://localhost:5182/api/Resume/UpsertResume",
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify(fileText),
          }
        );

        if (response.ok) {
          alert("Resume uploaded successfully!");
        } else {
          const result = await response.json();
          alert(`Error: ${result.message || result.Message}`);
        }
      } catch (error) {
        console.error("Error uploading resume:", error);
        alert(`Error processing file: ${error.message}`);
      } finally {
        setIsUploading(false);
      }
    }
  };

  const handleApiKeyChange = (event) => {
    setApiKey(event.target.value);
  };

  const handleApiKeySubmit = async () => {
    if (!apiKey.trim()) return alert("Please provide an API key");

    try {
      const response = await fetch(
        "http://localhost:5182/api/ApiKey/UpsertApiKey",
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ key: apiKey }),
        }
      );

      const result = await response.json();
      alert(
        response.ok
          ? "API key saved successfully!"
          : `Error: ${result.message || result.Message}`
      );
      if (response.ok) setApiKey("");
    } catch (error) {
      console.error("Error saving API key:", error);
      alert("Network error occurred. Please try again");
    }
  };

  return (
    <>
      <div className="app-container">
        <div className="main-card">
          <h1 className="title">
            Hello, this is <span className="title-highlight">Abssi</span>
          </h1>
          <p className="subtitle">Your assistant to find a job</p>

          <div className="button-container">
            <button className="button-base button-grey">Cover letter</button>
            <button className="button-base button-grey">Job matching</button>
          </div>

          {/* <p className="file-name">{resumeFileName}</p> */}
          <p className="resume-text">
            {isResumeSaved ? "Resume uploaded ✅" : "No resume uploaded yet"}
          </p>

          <label className="upload-label button-base button-purple">
            <input
              type="file"
              onChange={handleResumeUpload}
              className="input-file"
              accept=".pdf,.txt"
              disabled={isUploading}
            />
            {isUploading ? "Processing..." : "Upload your Resume"}
          </label>

          <p className="api-key-text">
            {isApiKeySaved ? "API key added ✅" : "Add your Gemini API key"}
          </p>
          <div className="api-key-input-container">
            <input
              type="password"
              value={apiKey}
              onChange={handleApiKeyChange}
              placeholder="Enter API key"
              className="api-key-input"
            />
            <button
              onClick={handleApiKeySubmit}
              className="button-base button-purple"
            >
              Submit
            </button>
          </div>
        </div>
      </div>
    </>
  );
};

export default App;

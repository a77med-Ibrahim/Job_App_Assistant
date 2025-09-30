import React, { useState, useEffect } from "react";
import "./CoverLetter.css";

export default function App() {
  const [isLoading, setIsLoading] = useState(false);
  const [coverLetterJobDesc, setCoverLetterJobDesc] = useState("");
  const [additionalDetails, setAdditionalDetails] = useState("");
  const [generatedCoverLetter, setGeneratedCoverLetter] = useState("");
  const [copyButtonText, setCopyButtonText] = useState("Copy");

  useEffect(() => {
    const script = document.createElement("script");
    script.src =
      "https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js";
    script.async = true;
    document.body.appendChild(script);
    return () => {
      document.body.removeChild(script);
    };
  }, []);
  const handleCopy = () => {
    navigator.clipboard
      .writeText(generatedCoverLetter)
      .then(() => {
        setCopyButtonText("Copied!");
        setTimeout(() => setCopyButtonText("Copy"), 2000);
      })
      .catch((err) => {
        console.error("Failed to copy text:", err);
        alert("Copy failed. Please try again.");
      });
  };

  const handleGenerateCoverLetter = async () => {
    if (!coverLetterJobDesc.trim()) {
      alert("Please paste a job description.");
      return;
    }

    setIsLoading(true);
    setGeneratedCoverLetter("");

    try {
      const response = await fetch(
        "http://localhost:5182/api/CoverLetter/CoverLetter",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            JobDescription: coverLetterJobDesc,
            additionalDetails: additionalDetails,
          }),
        }
      );

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText);
      }

      const data = await response.json();
      setGeneratedCoverLetter(data.coverLetter);
    } catch (err) {
      console.error("Error generating cover letter:", err);
      alert("Failed to generate cover letter. Check console for details.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleDownloadTxt = () => {
    const element = document.createElement("a");
    const file = new Blob([generatedCoverLetter], { type: "text/plain" });
    element.href = URL.createObjectURL(file);
    element.download = "cover-letter.txt";
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
  };

  const handleDownloadPdf = () => {
    if (window.jspdf) {
      const { jsPDF } = window.jspdf;
      const doc = new jsPDF();
      const splitText = doc.splitTextToSize(generatedCoverLetter, 180);
      doc.text(splitText, 15, 20);
      doc.save("cover-letter.pdf");
    } else {
      alert("PDF library is still loading. Please try again in a moment.");
    }
  };
  const LoadingSpinner = ({ text = "Generating..." }) => (
    <div className="loading-spinner-container">
      <div className="spinner-outer"></div>
      <div className="spinner-inner"></div>
      <p className="spinner-text">{text}</p>
    </div>
  );
  return (
    <div className="app-container">
      <div className="card">
        <div className="animate-fade-in">
          <h1 className="title">
            Cover Letter <span className="title-highlight">Generator</span>
          </h1>
          <p className="subtitle">
            Provide the job details to generate a professional cover letter.
          </p>

          <div className="grid-container">
            <div className="input-section">
              <div className="input-group">
                <h3 className="label">Job Description</h3>
                <textarea
                  value={coverLetterJobDesc}
                  onChange={(e) => setCoverLetterJobDesc(e.target.value)}
                  placeholder="Paste the job description..."
                  className="textarea textarea-jd"
                ></textarea>
              </div>
              <div className="input-group">
                <h3 className="label">Additional Details</h3>
                <textarea
                  value={additionalDetails}
                  onChange={(e) => setAdditionalDetails(e.target.value)}
                  placeholder="e.g., mention my 3 years of experience with React..."
                  className="textarea textarea-details"
                ></textarea>
              </div>
              <button
                onClick={handleGenerateCoverLetter}
                disabled={isLoading}
                className="button-primary"
              >
                {isLoading ? "Generating..." : "Generate Cover Letter"}
              </button>
            </div>
            <div className="output-section">
              <div className="output-box">
                {isLoading ? (
                  <LoadingSpinner text="Generating..." />
                ) : generatedCoverLetter ? (
                  <textarea
                    readOnly
                    value={generatedCoverLetter}
                    className="output-textarea"
                  ></textarea>
                ) : (
                  <div className="output-placeholder">
                    Your cover letter will appear here...
                  </div>
                )}
              </div>
              {generatedCoverLetter && !isLoading && (
                <div className="actions-container">
                  <button onClick={handleCopy} className="button-secondary">
                    {copyButtonText}
                  </button>
                  <button
                    onClick={handleDownloadTxt}
                    className="button-secondary"
                  >
                    Download .txt
                  </button>
                  <button
                    onClick={handleDownloadPdf}
                    className="button-secondary"
                  >
                    Download .pdf
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

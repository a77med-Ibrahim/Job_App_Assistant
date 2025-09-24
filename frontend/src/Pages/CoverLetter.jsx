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

  const handleGenerateCoverLetter = () => {
    if (!coverLetterJobDesc.trim()) {
      alert("Please paste a job description.");
      return;
    }
    setIsLoading(true);
    setGeneratedCoverLetter("");
    setTimeout(() => {
      const placeholderText = `Dear Hiring Manager,\n\nI am writing to express my keen interest in the position advertised. Based on the job description, I am confident that my skills and experience are a strong match for this role.\n\nMy background in [Your Field] has prepared me to excel in a position like this one. I am particularly adept at [Skill 1], [Skill 2], and [Skill 3]. ${
        additionalDetails
          ? `Furthermore, regarding the details I provided: ${additionalDetails}`
          : ""
      }\n\nI am excited about the opportunity to contribute to your team and am eager to discuss my application further. Thank you for your time and consideration.\n\nSincerely,\n[Your Name]`;
      setGeneratedCoverLetter(placeholderText);
      setIsLoading(false);
    }, 2500);
  };
  const handleCopy = () => {
    if (generatedCoverLetter) {
      const textArea = document.createElement("textarea");
      textArea.value = generatedCoverLetter;
      textArea.style.position = "fixed";
      textArea.style.left = "-9999px";
      document.body.appendChild(textArea);
      textArea.select();
      try {
        document.execCommand("copy");
        setCopyButtonText("Copied!");
        setTimeout(() => setCopyButtonText("Copy"), 2000);
      } catch (err) {
        console.error("Unable to copy", err);
      }
      document.body.removeChild(textArea);
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

import React, { useState, useRef, useEffect } from "react";

// Main App Component
export default function App() {
  // State for user inputs and results
  const [apiKey, setApiKey] = useState("");
  const [tempApiKey, setTempApiKey] = useState("");
  const [resumeFile, setResumeFile] = useState(null);
  const [jobDescription, setJobDescription] = useState("");
  const [matchingScore, setMatchingScore] = useState(null);
  const [isLoading, setIsLoading] = useState(false);

  // Ref for the hidden file input
  const fileInputRef = useRef(null);

  // --- Event Handlers ---

  const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (file) {
      setResumeFile(file);
    }
  };

  const handleApiKeySubmit = (e) => {
    e.preventDefault();
    setApiKey(tempApiKey);
  };

  // Simulates an API call to calculate the job match score
  const handleCalculateScore = () => {
    if (!jobDescription.trim()) {
      // Using a custom alert modal instead of window.alert
      alert("Please paste a job description.");
      return;
    }
    if (!resumeFile) {
      alert("Please upload your resume first.");
      return;
    }

    setIsLoading(true);
    setMatchingScore(null);

    // Simulate network delay and calculation
    setTimeout(() => {
      const randomScore = Math.floor(Math.random() * (99 - 75 + 1)) + 75;
      setMatchingScore(randomScore);
      setIsLoading(false);
    }, 2500);
  };

  // --- Helper Components / Render Functions ---

  // Animated circle for displaying the score
  const ScoreCircle = ({ score }) => {
    const [displayScore, setDisplayScore] = useState(0);
    const circumference = 2 * Math.PI * 55; // 55 is the radius
    const offset = circumference - (displayScore / 100) * circumference;

    useEffect(() => {
      if (score === null) {
        setDisplayScore(0);
        return;
      }
      const increment = score / 50;
      const interval = setInterval(() => {
        setDisplayScore((prev) => {
          if (prev < score) {
            return Math.min(prev + increment, score);
          }
          clearInterval(interval);
          return score;
        });
      }, 30);
      return () => clearInterval(interval);
    }, [score]);

    return (
      <div className="score-circle-wrapper">
        <svg className="score-circle-svg" viewBox="0 0 120 120">
          <circle
            cx="60"
            cy="60"
            r="55"
            strokeWidth="10"
            className="score-circle-bg"
            fill="transparent"
          />
          <circle
            cx="60"
            cy="60"
            r="55"
            strokeWidth="10"
            className="score-circle-fg"
            fill="transparent"
            strokeDasharray={circumference}
            strokeDashoffset={offset}
            strokeLinecap="round"
          />
        </svg>
        <div className="score-text-container">
          <span className="score-percentage">{Math.round(displayScore)}%</span>
          <p className="score-label">Match Score</p>
        </div>
      </div>
    );
  };

  // Loading spinner
  const LoadingSpinner = () => (
    <div className="spinner-wrapper">
      <div className="spinner-track"></div>
      <div className="spinner-head"></div>
      <p className="spinner-text">Analyzing...</p>
    </div>
  );

  return (
    <div className="app-container">
      {/* Main container card */}
      <div className="main-card">
        <div className="animate-fade-in">
          <h1 className="title">
            Job <span className="title-highlight">Matching</span>
          </h1>
          <p className="subtitle">
            Upload your resume and a job description to see your match score.
          </p>

          <div className="content-grid">
            {/* Left side: Inputs */}
            <div className="inputs-container">
              {/* Resume Upload */}
              <div className="upload-container">
                {resumeFile ? (
                  <div className="file-display">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="20"
                      height="20"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      className="file-icon-success"
                    >
                      <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
                      <polyline points="22 4 12 14.01 9 11.01" />
                    </svg>
                    <p className="file-name">{resumeFile.name}</p>
                  </div>
                ) : (
                  <p className="upload-prompt">Upload your Resume</p>
                )}
                <input
                  type="file"
                  ref={fileInputRef}
                  onChange={handleFileChange}
                  accept=".pdf,.doc,.docx"
                  className="hidden-file-input"
                />
                <button
                  onClick={() => fileInputRef.current.click()}
                  className="button button-secondary"
                >
                  {resumeFile ? "Change Resume" : "Select File"}
                </button>
              </div>

              {/* Job Description */}
              <div className="input-field">
                <h3 className="input-label">Paste Job Description Here</h3>
                <textarea
                  value={jobDescription}
                  onChange={(e) => setJobDescription(e.target.value)}
                  placeholder="Paste the full job description here..."
                  className="textarea-input"
                ></textarea>
              </div>

              {/* API Key */}
              <div className="input-field">
                <p className="input-label-sm">
                  {apiKey ? `API Key Saved!` : `Add your Gemini API key`}
                </p>
                <form onSubmit={handleApiKeySubmit} className="api-key-form">
                  <input
                    type="password"
                    value={tempApiKey}
                    onChange={(e) => setTempApiKey(e.target.value)}
                    placeholder="Enter API Key"
                    className="text-input"
                  />
                  <button type="submit" className="button button-tertiary">
                    Submit
                  </button>
                </form>
              </div>
            </div>

            {/* Right side: Output & Action */}
            <div className="output-container">
              <div className="score-display-area">
                {isLoading ? (
                  <LoadingSpinner />
                ) : (
                  <ScoreCircle score={matchingScore} />
                )}
              </div>
              <button
                onClick={handleCalculateScore}
                disabled={isLoading}
                className="button button-accent"
              >
                {isLoading ? "Calculating..." : "Calculate Score"}
              </button>
            </div>
          </div>
        </div>
      </div>

      <CustomStyles />
    </div>
  );
}

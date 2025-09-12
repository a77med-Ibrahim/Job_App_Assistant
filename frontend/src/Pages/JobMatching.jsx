import React, { useState, useRef, useEffect } from "react";
import "./JobMatching.css";

export default function App() {
  const [jobDescription, setJobDescription] = useState("");
  const [matchingScore, setMatchingScore] = useState(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleCalculateScore = () => {
    if (!jobDescription.trim()) {
      alert("Please paste a job description.");
      return;
    }

    setIsLoading(true);
    setMatchingScore(null);
    setTimeout(() => {
      const randomScore = Math.floor(Math.random() * (99 - 75 + 1)) + 75;
      setMatchingScore(randomScore);
      setIsLoading(false);
    }, 2500);
  };

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

  const LoadingSpinner = () => (
    <div className="spinner-wrapper">
      <div className="spinner-track"></div>
      <div className="spinner-head"></div>
      <p className="spinner-text">Analyzing...</p>
    </div>
  );

  return (
    <div className="app-container">
      <div className="main-card">
        <div className="animate-fade-in">
          <h1 className="title">
            Job <span className="title-highlight">Matching</span>
          </h1>
          <p className="subtitle">
            Upload your Job description to see your match score.
          </p>

          <div className="content-grid">
            <div className="input-field">
              <h3 className="input-label">Paste Job Description Here</h3>
              <textarea
                value={jobDescription}
                onChange={(e) => setJobDescription(e.target.value)}
                placeholder="Paste the full job description here..."
                className="textarea-input"
              ></textarea>
            </div>
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
    </div>
  );
}

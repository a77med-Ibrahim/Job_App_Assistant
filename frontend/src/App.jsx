import React, { useState } from "react";
import "./App.css";

const App = () => {
  const [resumeFileName, setResumeFileName] = useState("No file uploaded");
  const [apiKey, setApiKey] = useState("");

  const handleResumeUpload = (event) => {
    if (event.target.files.length > 0) {
      setResumeFileName(event.target.files[0].name);
    }
  };

  const handleApiKeyChange = (event) => {
    setApiKey(event.target.value);
  };

  const handleApiKeySubmit = () => {
    console.log("Submitting API Key:", apiKey);
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

          <p className="file-name">{resumeFileName}</p>

          <label className="upload-label button-base button-purple">
            <input
              type="file"
              onChange={handleResumeUpload}
              className="input-file"
            />
            Upload your Resume
          </label>

          <p className="api-key-text">Add your Gemini API key</p>
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

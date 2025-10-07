# Abssi – AI Job Application Assistant

**Abssi** is a full-stack web project built to showcase the integration of AI agents in real-world applications.  
It demonstrates how artificial intelligence can assist users in job applications by generating personalized cover letters and calculating job match scores efficiently and affordably.

---

## 🧩 Tech Stack

- **ASP.NET Core** – Backend API  
- **Entity Framework Core** – Database / ORM  
- **ReactJS** – Frontend Framework  
- **SQLite** – Lightweight local database  
- **Gemini 2.5 Flash API** – AI agent for resume analysis and cover letter generation  

---

## ⚙️ Supported AI Model

### Gemini 2.5 Flash Only

This project currently supports **Gemini 2.5 Flash** exclusively.

#### Why Gemini 2.5 Flash?

- It’s **fast**, **cost-efficient**, and **token-light**.  
- Job seekers often apply to many positions, so minimizing token usage helps reduce cost and improve response time.

#### Why Gemini instead of OpenAI or others?

Because **Gemini’s API keys are free** for anyone through [Google AI Studio](https://aistudio.google.com/), making this project accessible to all developers and users.

---

## 💡 Features

The website provides two main tools:

### 1. Cover Letter Writer
Users upload their **resume** and **job description**.  
They can also add extra personal information not present in the resume.  
The AI then generates a **personalized cover letter** for that specific job.

### 2. Job Match Score
Users provide their **resume** and **job description**, and the AI calculates a **match percentage** based on:
- Job and user **location**
- **Languages spoken**
- **University degree**
- **Years of experience**
- **Field of experince**
- **Skills and tools**

This helps users quickly identify which jobs are the best fit.

---

## 🖼️ Screenshots

### 1. Main Page  
Users can enter their **API key** and upload their **resume** to start using the assistant.  
![Main Page Screenshot](./screenshots/main-page.png)

---

### 2. Cover Letter Generator  
Includes fields for the **job description**, **extra information**, and a section where the **AI-generated cover letter** appears.  
![Cover Letter Page Screenshot](./screenshots/cover-letter-page.png)

---

### 3. Job Match Scoring  
Users enter a **job description**, and the AI provides a **match score** based on resume data, experience, location, and skills.  
![Job Match Page Screenshot](./screenshots/job-match-page.png)

---

## 🧠 Engineering Behind the Idea

Applying to many jobs using full resumes (often 1.5–2 pages long) is inefficient — it consumes **thousands of tokens** per request.  
To solve this, Abssi introduces a **shortResume** optimization technique.

### How It Works
1. The AI processes the user’s full resume once and extracts key details:
   - Location  
   - Languages spoken  
   - Years of experience  
   - University and education  
   - Field of experince 

2. These details are stored in the database as a text summary field called **`shortResume`**, which is only **3–5 sentences long**.

3. For subsequent actions (like job match or cover letter generation), the system uses **shortResume** instead of the full resume.

### Benefits
- Reduces input token usage from **1,500–2,000 tokens** → **100–130 tokens** per request.  
- Saves significant API costs.  
- Speeds up response time.  
- Maintains accuracy and relevance in AI results.

---

## 🚀 Setup / Installation

### Backend
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```
### Frontend
```bash
cd frontend
npm install
npm start
```

## ⚠️ Important Note

While this AI-powered tool helps generate ideas and drafts, it’s always best to write your own authentic cover letter that reflects your personality and values.

This project was built for learning, experimentation, and demonstration purposes.

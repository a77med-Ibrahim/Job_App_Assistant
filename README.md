# Abssi – AI Job Application Assistant

This is not the first project of its kind,  

But it’s the most convenient, easy to use, requires no subscription, and no effort from you.  

Abssi is your AI mate that checks your resume against the job description you are applying for, and tells you how well your resume matches the job.

Not only that, it also writes a very detailed cover letter that covers all the main points you can use to apply for your dream job. There’s even a field where you can add any extra information you want to include in the cover letter that isn’t in your resume.

*Whisper*: it doesn’t steal your information or give it to third parties. Cough, except for Google :)

Check the following repository for more technical details, and to clone the project and use it locally:

https://github.com/a77med-Ibrahim/Job_App_Assistant

---

## Features
- Resume analysis against job descriptions
- Resume match scoring based on location, years of experience, certificates, and languages
- AI-generated detailed cover letter
- Optional field to add extra information not included in the resume
- Runs fully locally, no subscription needed

---

## Tech Stack
- **ASP.NET Core** – Backend API
- **Entity Framework Core** – Database / ORM
- **ReactJS** – Frontend
- **SQLite** – Lightweight local database
- **Gemini 2.5 Flash API** – AI agent for resume analysis and cover letter generation

### About Gemini 2.5 Flash API
- Used because it is fast, accurate, and anyone can get a free API key from [AI Studio](https://aistudio.google.com/)
- The AI was given a precise prompt to effectively perform the tasks required
- To reduce token usage, when a user uploads their resume, Abssi extracts only the important keywords (e.g., location, years of experience, certificates, skills) to create a condensed version
- This condensed resume is then compared with the job description, saving thousands of tokens instead of processing the full resume each time

---

## Setup / Installation

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


### Important Note

Always keep in mind that writing your own authentic cover letter is better than taking one from AI.

This project was made for fun and learning purposes only.




# 🎓 Course Management System 📚

## 📌 Overview

This desktop-based **Course Management System** was developed using **C# (Windows Forms)** with a **Microsoft SQL Server** backend. It supports instructors and students in managing courses, lessons, exams, assignments, and communication.

The system was built following **Object-Oriented Programming (OOP)** principles to ensure clarity, scalability, and maintainability.

### 👨‍🏫 System Highlights

- Register as **student** or **instructor**
- Instructors can manage courses, exams, and assignments
- Students can register for courses and track their performance
- Both users can communicate through an internal **chat system**
- Lessons and course materials can be added to each course

---

## ✅ Key Features

- ✅ User registration (student/instructor) and profile updates  
- ✅ Add and manage courses (by instructors)  
- ✅ Link exams to courses  
- ✅ Student course registration with grade tracking  
- ✅ Detailed exam creation (questions, duration, date)  
- ✅ Classwork (assignments) creation linked to courses  
- ✅ Lesson and course content management  
- ✅ Chat between users with timestamps and sender/receiver info  

---

## 🧾 Functional Requirements Mapping

| Functionality                  | Description                                                                 |
|-------------------------------|-----------------------------------------------------------------------------|
| Register a new user           | Save user data (name, role, email, password) into the `Userr` table         |
| Update user profile           | Update fields like email, password, role in the `Userr` table              |
| Add/edit course (instructor)  | Instructors create/update course data like title, description, semester     |
| Assign exam to course         | Exams are stored in `Exam` and linked to courses via `ExamId`              |
| Register student in course    | Students are linked to courses in `Register`, along with their grades      |
| Add exam details              | Exams include questions, duration, and date stored in the `Exam` table     |
| Create classwork (assignment) | Assignments are linked to courses via composite keys (CourseId, ExId)      |
| Add lessons or content        | Lessons are added to courses with content and dates via the `Lesson` table |
| Chat between users            | Messaging stored in `Message`, linked to chats and users with timestamps   |

---

## ⚙️ How It Works

1. Users register and log in as **students** or **instructors**
2. Instructors can:
   - Add and update courses
   - Assign exams and classwork
   - Add lessons and content
3. Students can:
   - Register in courses
   - View grades
4. Both can chat in real-time
5. All actions are handled through a robust **SQL Server** database

---

## 🛠️ Technologies & Tools Used

- 💻 **C# (Windows Forms)** – GUI & application logic  
- 🗄️ **SQL Server** – Database backend  
- 🧰 **Visual Studio** – Development environment  
- 🧩 **Draw.io & PowerDesigner** – ERD (conceptual & physical) design  
- 🌐 **GitHub** – Source control & collaboration  
- 📝 **Microsoft Word** – Documentation & reporting  

---

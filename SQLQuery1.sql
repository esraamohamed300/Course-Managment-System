use course_system;
Go

CREATE TABLE Userr (
    UserId INT PRIMARY KEY,
    FName VARCHAR(30),
	LName VARCHAR(30) ,
    User_Role VARCHAR(10) ,
    Pass_word VARCHAR(12) ,
    Email VARCHAR(30) 
);

CREATE TABLE Exam (
    ExamId INT PRIMARY KEY,
    Description VARCHAR(100),
    Question VARCHAR(256)  ,
    Duration VARCHAR(20) ,
    Title VARCHAR(20) ,
    Date DATE 
);

CREATE TABLE Course (
    CourseId INT PRIMARY KEY,
    UserId INT ,
    ExamId INT ,
    Title VARCHAR(20) ,
    Description VARCHAR(100),
    Category VARCHAR(30) ,
    Studying_Year INT,
    Semester INT ,
    is_hidden SMALLINT,

    FOREIGN KEY (UserId) REFERENCES Userr(UserId),
    FOREIGN KEY (ExamId) REFERENCES Exam(ExamId)
);

CREATE TABLE Register (
    UserId INT,
    CourseId INT,
    Grade INT ,
    PRIMARY KEY (UserId, CourseId),
    FOREIGN KEY (UserId) REFERENCES Userr(UserId),
    FOREIGN KEY (CourseId) REFERENCES Course(CourseId)
);

CREATE TABLE Lesson (
    LessonId INT PRIMARY KEY,
    CourseId INT,
    Title VARCHAR(20) ,
    Content VARCHAR(256) ,
    Date DATE,
    FOREIGN KEY (CourseId) REFERENCES Course(CourseId)
);

CREATE TABLE ClassWork (
    CourseId INT,
    ExId INT,
    Title VARCHAR(20),
    Duration varchar(10) ,
    Date DATE,
    Description varchar(256) ,
    PRIMARY KEY (CourseId, ExId),
    FOREIGN KEY (CourseId) REFERENCES Course(CourseId),
);


CREATE TABLE Chat (
    ChatId INT PRIMARY KEY,
    Sender_id INT,
    Reciever_id INT ,
    FOREIGN KEY (Sender_id) REFERENCES Userr(UserId),
    FOREIGN KEY (Reciever_id) REFERENCES Userr(UserId)
);


CREATE TABLE Message (
    MessageId INT PRIMARY KEY,
    UserId INT,
    Content VARCHAR(256) ,
    Subject VARCHAR(100),
    FOREIGN KEY (UserId) REFERENCES Userr(UserId)
);

Alter Table Course
Add Cover varchar(100);

Alter Table ClassWork
Alter Column Duration varchar(10);



ALTER TABLE Course ADD Cover VARCHAR(100);

--delete pr key in chat table chatid

ALTER TABLE Chat ALTER COLUMN Sender_id INT NOT NULL;
ALTER TABLE Chat ALTER COLUMN Reciever_id INT NOT NULL;



Drop table Message

CREATE TABLE Message (
    MessageId INT PRIMARY KEY,
    Sender_id INT,
	Reciever_id INT,
	timee time not null,
	Datee date not null,
    Content VARCHAR(256) ,
    Subject VARCHAR(100),
    FOREIGN KEY (Sender_id) REFERENCES Userr(UserId),
	FOREIGN KEY (Reciever_id) REFERENCES Userr(UserId)
);

ALTER TABLE Chat DROP Constraint pk1;

ALTER TABLE Chat DROP COLUMN ChatId;

ALTER TABLE Chat ADD ChatId INT IDENTITY(1,1);

ALTER TABLE Chat 
ADD CONSTRAINT pk1 PRIMARY KEY (Sender_id, Reciever_id, ChatId);

ALTER TABLE Chat ADD Subject Varchar(50);
Alter Table Message
Drop column Subject

UPDATE Course SET cover = NULL;
ALTER TABLE Course
Drop Column Cover

ALTER TABLE Course
Add  Cover VARBINARY(MAX);

--------------------------------------------------

ALTER TABLE Exam
Add  Answers VARchar(256);


ALTER TABLE ClassWork
Add  Answerpdf VARBINARY(MAX);
------------------------------------
--new
ALTER TABLE Lesson
Add  media VARBINARY(MAX);

Alter Table Lesson Drop constraint PK__Lesson__B084ACD056AAACE4
ALTER TABLE Lesson DROP COLUMN LessonId;

ALTER TABLE Lesson ADD LessonId INT IDENTITY(1,1);

ALTER TABLE Lesson ADD CONSTRAINT pk2  PRIMARY KEY (LessonId);

-----------------------------

Alter Table Course Add  PassedExam_ID  INT 
ALTER TABLE Course ADD CONSTRAINT FK2  foreign KEY (PassedExam_ID) References Exam(ExamId);

Alter Table ClassWork Drop constraint PK__ClassWor__0A4A4259E135E773
ALTER TABLE ClassWork ADD CONSTRAINT pk3 PRIMARY KEY (ExId) ;
--------------------------------

Alter Table ClassWork
ALTER COLUMN Answerpdf varchar(200)

---------------------------------------
ALTER TABLE Message DROP Constraint PK__Message__C87C0C9CB8FC7BFB;

ALTER TABLE Message DROP COLUMN MessageId;

ALTER TABLE Message ADD MessageId INT IDENTITY(1,1);

ALTER TABLE Message 
ADD CONSTRAINT pk5 PRIMARY KEY (MessageId);


--new
Alter Table Register drop constraint PK__Register__7B1A1B566BAAA27C;


ALTER TABLE Register ADD CONSTRAINT pk6 PRIMARY KEY (UserId) ;

------------------------------

Alter Table Register drop constraint pk6;
-------------------------
ALTER TABLE Register ADD CONSTRAINT pk6 PRIMARY KEY (UserId,CourseId)

----------------



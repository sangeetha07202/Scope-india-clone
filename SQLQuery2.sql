DELETE FROM Students WHERE Email='gk9085549@gmail.com'; 


SELECT * FROM Students WHERE Email='gk9085549@gmail.com'; 


CREATE TABLE Student (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(100),
    Email NVARCHAR(150) UNIQUE,
    Password NVARCHAR(100),
    ConfirmPassword NVARCHAR(100)
);

INSERT INTO Student (UserName, Email, Password, ConfirmPassword)
VALUES ('Kalpss', 'kalpss29@gmail.com', '12345', '12345');
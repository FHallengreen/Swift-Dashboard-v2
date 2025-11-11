USE swift_dashboard;

CREATE TABLE Invoices (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Year INT NOT NULL,
    Month INT NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    UNIQUE KEY idx_year_month (Year, Month)
);

CREATE TABLE Info (
    Id INT PRIMARY KEY,
    Text TEXT
);

-- Initial data for Info table
INSERT INTO Info (Id, Text) VALUES (1, 'Welcome to Swift Display Dashboard. This is a demo system.');
-- Generate randomized invoice data for 2024 (all 12 months)
-- Values range from 600,000 to 1,600,000 to simulate realistic business data
INSERT IGNORE INTO Invoices (Year, Month, Amount)
VALUES
    (2024, 1, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 2, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 3, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 4, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 5, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 6, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 7, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 8, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 9, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 10, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 11, ROUND(600000 + (RAND() * 1000000), 2)),
    (2024, 12, ROUND(600000 + (RAND() * 1000000), 2));

-- Generate randomized invoice data for 2025 (January to October)
INSERT IGNORE INTO Invoices (Year, Month, Amount)
VALUES
    (2025, 1, ROUND(600000 + (RAND() * 1000000), 2)),
    (2025, 2, ROUND(600000 + (RAND() * 1000000), 2)),
    (2025, 3, ROUND(600000 + (RAND() * 1000000), 2)),
    (2025, 4, ROUND(600000 + (RAND() * 1000000), 2)),
    (2025, 5, ROUND(600000 + (RAND() * 1000000), 2)),
    (2025, 6, ROUND(600000 + (RAND() * 1000000), 2)),
    (2025, 7, ROUND(600000 + (RAND() * 1000000), 2)),
    (2025, 8, ROUND(600000 + (RAND() * 1000000), 2)),
    (2025, 9, ROUND(600000 + (RAND() * 1000000), 2)),
    (2025, 10, ROUND(600000 + (RAND() * 1000000), 2));
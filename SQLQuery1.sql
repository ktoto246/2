CREATE DATABASE УчетЗаймов;
GO

USE УчетЗаймов;
GO

CREATE TABLE Клиенты (
    ID_Клиента INT IDENTITY(1,1) PRIMARY KEY,
    ФИО VARCHAR(255) NOT NULL,
    Паспортные_Данные VARCHAR(50) UNIQUE NOT NULL,
    Телефон VARCHAR(20),
    Дата_Рождения DATE NOT NULL
);

CREATE TABLE Кредитные_Продукты (
    ID_Продукта INT IDENTITY(1,1) PRIMARY KEY,
    Название VARCHAR(255) NOT NULL,
    Процентная_Ставка DECIMAL(5, 2) NOT NULL,
    Макс_Сумма DECIMAL(15, 2) NOT NULL,
    Макс_Срок_Месяцев INT NOT NULL
);

CREATE TABLE Займы (
    ID_Займа INT IDENTITY(1,1) PRIMARY KEY,
    ID_Клиента INT REFERENCES Клиенты(ID_Клиента) ON DELETE CASCADE,
    ID_Продукта INT REFERENCES Кредитные_Продукты(ID_Продукта) ON DELETE CASCADE,
    Сумма_Займа DECIMAL(15, 2) NOT NULL,
    Дата_Выдачи DATE NOT NULL,
    Статус VARCHAR(50) DEFAULT 'Активен' 
);

CREATE TABLE Платежи (
    ID_Платежа INT IDENTITY(1,1) PRIMARY KEY,
    ID_Займа INT REFERENCES Займы(ID_Займа) ON DELETE CASCADE,
    Дата_Платежа DATE NOT NULL,
    Сумма_Платежа DECIMAL(15, 2) NOT NULL,
    Тип_Платежа VARCHAR(100) DEFAULT 'Основной долг'
);

INSERT INTO Клиенты (ФИО, Паспортные_Данные, Телефон, Дата_Рождения) VALUES
('Иванов Иван Иванович', '4510 123456', '+79991234567', '1990-05-15'),
('Петров Петр Петрович', '4511 654321', '+79001112233', '1985-11-20'),
('Сидорова Анна Сергеевна', '4512 777888', '+79505554433', '1995-02-10');

INSERT INTO Кредитные_Продукты (Название, Процентная_Ставка, Макс_Сумма, Макс_Срок_Месяцев) VALUES
('Потребительский стандарт', 14.50, 500000.00, 36),
('Быстрый нал', 25.00, 50000.00, 6),
('Ипотечный лайт', 9.80, 5000000.00, 240);

INSERT INTO Займы (ID_Клиента, ID_Продукта, Сумма_Займа, Дата_Выдачи) VALUES
(1, 1, 150000.00, '2024-01-10'),
(2, 2, 30000.00, '2024-03-01'),
(3, 3, 2500000.00, '2023-12-15');

INSERT INTO Платежи (ID_Займа, Дата_Платежа, Сумма_Платежа) VALUES
(1, '2024-02-10', 5500.00),
(1, '2024-03-10', 5500.00),
(2, '2024-04-01', 6200.00);
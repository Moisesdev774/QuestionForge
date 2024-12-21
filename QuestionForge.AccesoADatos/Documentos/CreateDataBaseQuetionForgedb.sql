CREATE DATABASE QuestionForgedb
go

USE QuestionForgedb
go
CREATE TABLE Usuario (
    [Id] INT IDENTITY PRIMARY KEY,
    [Nombre] NVARCHAR(50) UNIQUE NOT NULL,
    [Password] NVARCHAR(MAX) NOT NULL,
    [FechaRegistro] DATETIME DEFAULT GETDATE()
);

CREATE TABLE Pregunta (
    [Id] INT IDENTITY PRIMARY KEY,
    [IdUsuario] INT NOT NULL,
    [Titulo] NVARCHAR(250) NOT NULL,
    [Descripcion] NVARCHAR(MAX) NOT NULL,
    [FechaCreacion] DATETIME DEFAULT GETDATE(),
    [Cerrada] BIT DEFAULT 0,
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
);

CREATE TABLE Respuesta (
    [Id] INT IDENTITY PRIMARY KEY,
    [IdPregunta] INT NOT NULL,
    [IdUsuario] INT NOT NULL,
    [Contenido] NVARCHAR(MAX) NOT NULL,
    [FechaCreacion] DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (IdPregunta) REFERENCES Pregunta(Id),
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
);

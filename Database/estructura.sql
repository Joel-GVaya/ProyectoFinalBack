-- Opcional: Crear la base de datos
-- CREATE DATABASE IF NOT EXISTS ProyectoImagenes;
-- USE ProyectoImagenes;

DROP TABLE IF EXISTS `niveles_acceso`;
CREATE TABLE `niveles_acceso` (
    `id` int NOT NULL,
    `cantidad` int NOT NULL,
    PRIMARY KEY (`id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `usuarios`;
CREATE TABLE `usuarios` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(255) NOT NULL,
    `apellidos` varchar(255) DEFAULT NULL,
    `correo` varchar(255) NOT NULL,
    `edad` int DEFAULT NULL,
    `telefono` varchar(20) DEFAULT NULL,
    `password` varchar(255) NOT NULL,
    `nivelAcceso` int NOT NULL,
    `imagen` longtext,
    PRIMARY KEY (`id`),
    UNIQUE KEY `correo` (`correo`),
    KEY `nivelAcceso` (`nivelAcceso`),
    CONSTRAINT `usuarios_ibfk_1` FOREIGN KEY (`nivelAcceso`) REFERENCES `niveles_acceso` (`id`)
) ENGINE = InnoDB AUTO_INCREMENT = 7 DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `estilosDisponibles`;
CREATE TABLE `estilosDisponibles` (
    `id` varchar(255) NOT NULL,
    `nombre` varchar(255) NOT NULL,
    `promptImagen` text NOT NULL,
    `imagen` longtext,
    `promptTexto` varchar(5000) NOT NULL,
    PRIMARY KEY (`id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `imagenesGeneradas`;
CREATE TABLE `imagenesGeneradas` (
    `id` varchar(255) NOT NULL,
    `id_usuario` int NOT NULL,
    `publicada` tinyint(1) NOT NULL DEFAULT '0',
    `fecha` datetime NOT NULL,
    `estilo` varchar(255) NOT NULL,
    `imagen_base64` longtext NOT NULL,
    PRIMARY KEY (`id`),
    KEY `estilo` (`estilo`),
    KEY `id_usuario` (`id_usuario`),
    CONSTRAINT `imagenesGeneradas_ibfk_2` FOREIGN KEY (`estilo`) REFERENCES `estilosDisponibles` (`id`),
    CONSTRAINT `imagenesGeneradas_ibfk_3` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `imagenesSubidas`;
CREATE TABLE `imagenesSubidas` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ImagenBase64` longtext NOT NULL,
    `ImagenGeneradaId` varchar(255) DEFAULT NULL,
    `id_usuario` int NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `ImagenGeneradaId` (`ImagenGeneradaId`),
    KEY `id_usuario` (`id_usuario`),
    CONSTRAINT `imagenesSubidas_ibfk_1` FOREIGN KEY (`ImagenGeneradaId`) REFERENCES `imagenesGeneradas` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT `imagenesSubidas_ibfk_2` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id`)
) ENGINE = InnoDB AUTO_INCREMENT = 50 DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS `PromptsUsuariosSubidos`;
CREATE TABLE `PromptsUsuariosSubidos` (
    `id` int NOT NULL AUTO_INCREMENT,
    `prompt` text NOT NULL,
    `imagen_generada_id` varchar(50) DEFAULT NULL,
    `id_usuario` int NOT NULL,
    PRIMARY KEY (`id`),
    KEY `id_usuario` (`id_usuario`),
    KEY `imagen_generada_id` (`imagen_generada_id`),
    CONSTRAINT `PromptsUsuariosSubidos_ibfk_2` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id`),
    CONSTRAINT `PromptsUsuariosSubidos_ibfk_3` FOREIGN KEY (`imagen_generada_id`) REFERENCES `imagenesGeneradas` (`id`) ON DELETE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 16 DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci;

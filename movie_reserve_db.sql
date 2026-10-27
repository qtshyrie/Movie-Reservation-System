-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Oct 27, 2026 at 05:24 AM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `movie_reserve_db`
--

DELIMITER $$
--
-- Procedures
--
CREATE DEFINER=`root`@`localhost` PROCEDURE `book_seats` (IN `p_customer` VARCHAR(100), IN `p_showtime` INT, IN `p_seat_ids` VARCHAR(1000), OUT `p_booking_id` INT, OUT `p_message` VARCHAR(255))   BEGIN
    DECLARE v_requested INT DEFAULT 0;
    DECLARE v_available INT DEFAULT 0;
    DECLARE v_room      INT DEFAULT NULL;
    DECLARE v_taken     INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET p_booking_id = NULL;
        SET p_message = 'Booking failed: seats no longer available or invalid.';
    END;

    SET p_booking_id = NULL;
    SET p_message = '';

    -- Which room does this showtime use?
    SELECT screen_id INTO v_room FROM showtimes WHERE showtime_id = p_showtime;
    IF v_room IS NULL THEN
        SET p_message = 'Unknown showtime.';
    ELSE
        -- How many distinct valid seats were asked for?
        SELECT COUNT(DISTINCT st.seat_id) INTO v_requested
        FROM seats st
        WHERE st.screen_id = v_room
          AND FIND_IN_SET(st.seat_id, p_seat_ids);

        -- How many of those are still free?
        SELECT COUNT(DISTINCT st.seat_id) INTO v_available
        FROM seats st
        WHERE st.screen_id = v_room
          AND FIND_IN_SET(st.seat_id, p_seat_ids)
          AND NOT EXISTS (
                SELECT 1 FROM booking_seats b
                WHERE b.showtime_id = p_showtime AND b.seat_id = st.seat_id);

        -- How many were already taken (for a precise message)?
        SET v_taken = v_requested - v_available;

        IF v_requested = 0 THEN
            SET p_message = 'None of those seats belong to this room.';
        ELSEIF v_taken > 0 THEN
            SET p_message = CONCAT(v_taken, ' seat(s) already taken. Please choose again.');
        ELSE
            START TRANSACTION;

            INSERT INTO bookings (customer_name) VALUES (p_customer);
            SET p_booking_id = LAST_INSERT_ID();

            INSERT INTO booking_seats (booking_id, showtime_id, seat_id)
            SELECT p_booking_id, p_showtime, st.seat_id
            FROM seats st
            WHERE st.screen_id = v_room
              AND FIND_IN_SET(st.seat_id, p_seat_ids);

            COMMIT;
            SET p_message = 'OK';
        END IF;
    END IF;
END$$

DELIMITER ;

-- --------------------------------------------------------

--
-- Table structure for table `accounts`
--

CREATE TABLE `accounts` (
  `id` int(255) NOT NULL,
  `firstname` varchar(255) NOT NULL,
  `username` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `role` enum('staff','admin') NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `accounts`
--

INSERT INTO `accounts` (`id`, `firstname`, `username`, `password`, `role`) VALUES
(1, 'Gabriel', 'staff', '1234', 'staff'),
(2, 'Cristopher', 'staff', '1234', 'staff'),
(3, 'Julian', 'staff', '1234', 'staff'),
(4, 'Timothy', 'staff', '1234', 'staff'),
(5, 'Romeo', 'admin\r\n', '123456789', 'admin');

-- --------------------------------------------------------

--
-- Table structure for table `bookings`
--

CREATE TABLE `bookings` (
  `booking_id` int(11) NOT NULL,
  `customer_name` varchar(100) NOT NULL,
  `booking_date` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `bookings`
--

INSERT INTO `bookings` (`booking_id`, `customer_name`, `booking_date`) VALUES
(1, 'DEMO Juan Dela Cruz', '2026-10-24 01:02:32'),
(2, 'DEMO Maria Santos', '2026-10-24 01:02:32'),
(3, 'DEMO Pedro Reyes', '2026-10-24 01:02:32'),
(4, 'Ramirez', '2026-10-26 14:11:47'),
(5, 'GAb', '2026-10-26 14:12:19');

-- --------------------------------------------------------

--
-- Table structure for table `booking_seats`
--

CREATE TABLE `booking_seats` (
  `booking_id` int(11) NOT NULL,
  `showtime_id` int(11) NOT NULL,
  `seat_id` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `booking_seats`
--

INSERT INTO `booking_seats` (`booking_id`, `showtime_id`, `seat_id`) VALUES
(1, 5, 312),
(1, 5, 313),
(2, 5, 284),
(2, 5, 285),
(2, 5, 286),
(3, 5, 327),
(3, 5, 328),
(3, 10, 442),
(3, 10, 443),
(4, 5, 287),
(4, 5, 296),
(4, 5, 297),
(4, 5, 298),
(4, 5, 299),
(4, 5, 308),
(4, 5, 309),
(4, 5, 310),
(4, 5, 311),
(5, 6, 412),
(5, 6, 413),
(5, 6, 424),
(5, 6, 425),
(5, 6, 436),
(5, 6, 437),
(5, 6, 448),
(5, 6, 449),
(5, 6, 460),
(5, 6, 461);

-- --------------------------------------------------------

--
-- Table structure for table `movieslist`
--

CREATE TABLE `movieslist` (
  `movie_id` int(11) NOT NULL,
  `movie_title` varchar(150) NOT NULL,
  `genre` varchar(100) DEFAULT NULL,
  `synopsis` text DEFAULT NULL,
  `duration` int(11) DEFAULT NULL,
  `poster_url` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `movieslist`
--

INSERT INTO `movieslist` (`movie_id`, `movie_title`, `genre`, `synopsis`, `duration`, `poster_url`) VALUES
(1, '2 Valid IDs', 'Comedy', 'A comedy film about unexpected situations.', 110, 'https://www.cinemalaya.org/wp-content/uploads/2025/10/2-Valid-IDs-200x300.jpg'),
(2, 'Pee Mak', 'Horror Comedy', 'A humorous story with supernatural elements.', 115, 'https://contents.pep.ph/images2/images2/2025/03/27/pee-mak-1743039831.jpg'),
(3, 'Digger', 'Comedy', 'A story about ambition and unexpected challenges.', 120, 'https://www.theupcoming.co.uk/wp-content/uploads/2026/09/digger_vert_main_2764x4096_intl_vv-only-601x891.jpg'),
(4, '18th Rose', 'Romance Drama', 'A young person discovers love and follows a dream.', 100, 'https://m.media-amazon.com/images/M/MV5BMjg0ODBhZDMtZTc2OC00NWZiLTliYzgtOGU1MDAxMzhkNDhiXkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg'),
(5, 'The Lotto Winner', 'Drama', 'A family faces challenges after an unexpected event.', 105, 'https://tse4.mm.bing.net/th/id/OIP.xc13FFKgNFsM0fqYxbqHsgHaLH?r=0&rs=1&pid=ImgDetMain&o=7&rm=3'),
(6, 'This, That and Everything in Between', 'Drama', 'A Filipino drama about relationships.', 110, 'https://store.bookleafpub.com/cdn/shop/files/1774610058734_9789375434092_front_680ad3e7-38b6-4171-b94e-63503888b621.png?v=1777447158&width=1445'),
(7, 'Meet, Greet & Bye', 'Comedy, Drama', 'A Filipino story about family and relationships.', 110, 'https://tse4.mm.bing.net/th/id/OIP.Hz9alhGS5uUA5Zk5fiAGZQHaNK?r=0&rs=1&pid=ImgDetMain&o=7&rm=3'),
(8, 'Sunshine', 'Drama', 'A story about dreams and challenges.', 100, 'https://cdn.manilastandard.net/wp-content/uploads/2025/06/IMG_3373.jpeg'),
(9, 'Kontrabida Academy', 'Comedy', 'A comedy about entering the world of villains.', 110, 'https://artworks.thetvdb.com/banners/v4/movie/364068/posters/689ffb415a528.jpg'),
(10, 'Un/Happy For You', 'Romance, Drama', 'Former lovers reconnect and face unresolved feelings.', 110, 'https://m.media-amazon.com/images/M/MV5BMGE4NTNhNTYtOWRhMS00NDBiLThmYWQtMzYxZjNjZDZlOGVjXkEyXkFqcGc@._V1_.jpg');

-- --------------------------------------------------------

--
-- Table structure for table `movie_reservations`
--

CREATE TABLE `movie_reservations` (
  `reservation_id` int(11) NOT NULL,
  `customer_name` varchar(100) NOT NULL,
  `movie_title` varchar(150) NOT NULL,
  `reservation_date` date NOT NULL,
  `show_time` time NOT NULL,
  `number_of_tickets` int(11) NOT NULL,
  `seat_number` varchar(20) DEFAULT NULL,
  `total_price` decimal(10,2) NOT NULL,
  `payment_status` varchar(20) DEFAULT 'Unpaid',
  `reservation_status` varchar(20) DEFAULT 'Pending',
  `genre` varchar(100) DEFAULT NULL,
  `synopsis` text DEFAULT NULL,
  `duration` int(11) DEFAULT NULL,
  `poster_url` varchar(255) DEFAULT NULL,
  `movie_id` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `movie_reservations`
--

INSERT INTO `movie_reservations` (`reservation_id`, `customer_name`, `movie_title`, `reservation_date`, `show_time`, `number_of_tickets`, `seat_number`, `total_price`, `payment_status`, `reservation_status`, `genre`, `synopsis`, `duration`, `poster_url`, `movie_id`) VALUES
(1, 'Daniel Baladad', '2 Valid IDs', '2026-10-25', '14:00:00', 2, 'A1,A2', 500.00, 'Unpaid', 'Pending', 'Comedy', 'A comedy film about unexpected situations.', 110, 'https://www.cinemalaya.org/wp-content/uploads/2025/10/2-Valid-IDs-200x300.jpg', 1),
(2, 'Jullian Millar', 'Pee Mak', '2026-10-25', '16:00:00', 2, 'B1,B2', 500.00, 'Unpaid', 'Pending', 'Horror Comedy', 'A humorous story with supernatural elements.', 115, 'https://contents.pep.ph/images2/images2/2025/03/27/pee-mak-1743039831.jpg', 2),
(3, 'Timothy Agravante', 'Digger', '2026-10-26', '18:00:00', 1, 'C1', 250.00, 'Unpaid', 'Pending', 'Comedy', 'A story about ambition and unexpected challenges.', 120, 'https://www.theupcoming.co.uk/wp-content/uploads/2026/09/digger_vert_main_2764x4096_intl_vv-only-601x891.jpg', 3),
(4, 'Cristopher Ramirez', '18th Rose', '2026-10-26', '19:00:00', 1, 'D1', 250.00, 'Unpaid', 'Pending', 'Romance Drama', 'A young person discovers love and follows a dream.', 100, 'https://m.media-amazon.com/images/M/MV5BMjg0ODBhZDMtZTc2OC00NWZiLTliYzgtOGU1MDAxMzhkNDhiXkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg', 4),
(5, 'John Gabriel Jeron', 'The Lotto Winner', '2026-10-27', '20:00:00', 2, 'E1,E2', 500.00, 'Unpaid', 'Pending', 'Drama', 'A family faces challenges after an unexpected event.', 105, 'https://tse4.mm.bing.net/th/id/OIP.xc13FFKgNFsM0fqYxbqHsgHaLH?r=0&rs=1&pid=ImgDetMain&o=7&rm=3', 5);

-- --------------------------------------------------------

--
-- Table structure for table `screens`
--

CREATE TABLE `screens` (
  `screen_id` int(11) NOT NULL,
  `screen_name` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `screens`
--

INSERT INTO `screens` (`screen_id`, `screen_name`) VALUES
(1, 'Screen 1'),
(2, 'Room1'),
(3, 'Room2'),
(4, 'Room3'),
(5, 'Room 4'),
(6, 'Room 5'),
(7, 'Room 6'),
(8, 'Room 7');

-- --------------------------------------------------------

--
-- Table structure for table `seats`
--

CREATE TABLE `seats` (
  `seat_id` int(11) NOT NULL,
  `screen_id` int(11) NOT NULL,
  `row_label` char(1) NOT NULL,
  `seat_number` int(11) NOT NULL,
  `seat_type` varchar(20) NOT NULL DEFAULT 'Regular'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `seats`
--

INSERT INTO `seats` (`seat_id`, `screen_id`, `row_label`, `seat_number`, `seat_type`) VALUES
(3, 1, 'A', 1, 'PWD/Elderly'),
(4, 1, 'A', 2, 'PWD/Elderly'),
(5, 1, 'A', 3, 'PWD/Elderly'),
(6, 1, 'A', 4, 'PWD/Elderly'),
(7, 1, 'A', 5, 'PWD/Elderly'),
(8, 1, 'A', 6, 'PWD/Elderly'),
(9, 1, 'A', 7, 'PWD/Elderly'),
(10, 1, 'A', 8, 'PWD/Elderly'),
(11, 1, 'A', 9, 'PWD/Elderly'),
(12, 1, 'A', 10, 'PWD/Elderly'),
(13, 1, 'A', 11, 'PWD/Elderly'),
(14, 1, 'A', 12, 'PWD/Elderly'),
(15, 1, 'B', 1, 'PWD/Elderly'),
(16, 1, 'B', 2, 'PWD/Elderly'),
(17, 1, 'B', 3, 'PWD/Elderly'),
(18, 1, 'B', 4, 'PWD/Elderly'),
(19, 1, 'B', 5, 'PWD/Elderly'),
(20, 1, 'B', 6, 'PWD/Elderly'),
(21, 1, 'B', 7, 'PWD/Elderly'),
(22, 1, 'B', 8, 'PWD/Elderly'),
(23, 1, 'B', 9, 'PWD/Elderly'),
(24, 1, 'B', 10, 'PWD/Elderly'),
(25, 1, 'B', 11, 'PWD/Elderly'),
(26, 1, 'B', 12, 'PWD/Elderly'),
(27, 1, 'C', 1, 'Regular'),
(28, 1, 'C', 2, 'Regular'),
(29, 1, 'C', 3, 'Regular'),
(30, 1, 'C', 4, 'Regular'),
(31, 1, 'C', 5, 'Regular'),
(32, 1, 'C', 6, 'Regular'),
(33, 1, 'C', 7, 'Regular'),
(34, 1, 'C', 8, 'Regular'),
(35, 1, 'C', 9, 'Regular'),
(36, 1, 'C', 10, 'Regular'),
(37, 1, 'C', 11, 'Regular'),
(38, 1, 'C', 12, 'Regular'),
(39, 1, 'D', 1, 'Regular'),
(40, 1, 'D', 2, 'Regular'),
(41, 1, 'D', 3, 'Regular'),
(42, 1, 'D', 4, 'Regular'),
(43, 1, 'D', 5, 'Regular'),
(44, 1, 'D', 6, 'Regular'),
(45, 1, 'D', 7, 'Regular'),
(46, 1, 'D', 8, 'Regular'),
(47, 1, 'D', 9, 'Regular'),
(48, 1, 'D', 10, 'Regular'),
(49, 1, 'D', 11, 'Regular'),
(50, 1, 'D', 12, 'Regular'),
(51, 1, 'E', 1, 'Regular'),
(52, 1, 'E', 2, 'Regular'),
(53, 1, 'E', 3, 'Regular'),
(54, 1, 'E', 4, 'Regular'),
(55, 1, 'E', 5, 'Regular'),
(56, 1, 'E', 6, 'Regular'),
(57, 1, 'E', 7, 'Regular'),
(58, 1, 'E', 8, 'Regular'),
(59, 1, 'E', 9, 'Regular'),
(60, 1, 'E', 10, 'Regular'),
(61, 1, 'E', 11, 'Regular'),
(62, 1, 'E', 12, 'Regular'),
(63, 1, 'F', 1, 'Regular'),
(64, 1, 'F', 2, 'Regular'),
(65, 1, 'F', 3, 'Regular'),
(66, 1, 'F', 4, 'Regular'),
(67, 1, 'F', 5, 'Regular'),
(68, 1, 'F', 6, 'Regular'),
(69, 1, 'F', 7, 'Regular'),
(70, 1, 'F', 8, 'Regular'),
(71, 1, 'F', 9, 'Regular'),
(72, 1, 'F', 10, 'Regular'),
(73, 1, 'F', 11, 'Regular'),
(74, 1, 'F', 12, 'Regular'),
(75, 1, 'G', 1, 'Regular'),
(76, 1, 'G', 2, 'Regular'),
(77, 1, 'G', 3, 'Regular'),
(78, 1, 'G', 4, 'Regular'),
(79, 1, 'G', 5, 'Regular'),
(80, 1, 'G', 6, 'Regular'),
(81, 1, 'G', 7, 'Regular'),
(82, 1, 'G', 8, 'Regular'),
(83, 1, 'G', 9, 'Regular'),
(84, 1, 'G', 10, 'Regular'),
(85, 1, 'G', 11, 'Regular'),
(86, 1, 'G', 12, 'Regular'),
(87, 1, 'H', 1, 'Regular'),
(88, 1, 'H', 2, 'Regular'),
(89, 1, 'H', 3, 'Regular'),
(90, 1, 'H', 4, 'Regular'),
(91, 1, 'H', 5, 'Regular'),
(92, 1, 'H', 6, 'Regular'),
(93, 1, 'H', 7, 'Regular'),
(94, 1, 'H', 8, 'Regular'),
(95, 1, 'H', 9, 'Regular'),
(96, 1, 'H', 10, 'Regular'),
(97, 1, 'H', 11, 'Regular'),
(98, 1, 'H', 12, 'Regular'),
(99, 1, 'I', 1, 'Regular'),
(100, 1, 'I', 2, 'Regular'),
(101, 1, 'I', 3, 'Regular'),
(102, 1, 'I', 4, 'Regular'),
(103, 1, 'I', 5, 'Regular'),
(104, 1, 'I', 6, 'Regular'),
(105, 1, 'I', 7, 'Regular'),
(106, 1, 'I', 8, 'Regular'),
(107, 1, 'I', 9, 'Regular'),
(108, 1, 'I', 10, 'Regular'),
(109, 1, 'I', 11, 'Regular'),
(110, 1, 'I', 12, 'Regular'),
(111, 1, 'J', 1, 'Regular'),
(112, 1, 'J', 2, 'Regular'),
(113, 1, 'J', 3, 'Regular'),
(114, 1, 'J', 4, 'Regular'),
(115, 1, 'J', 5, 'Regular'),
(116, 1, 'J', 6, 'Regular'),
(117, 1, 'J', 7, 'Regular'),
(118, 1, 'J', 8, 'Regular'),
(119, 1, 'J', 9, 'Regular'),
(120, 1, 'J', 10, 'Regular'),
(121, 1, 'J', 11, 'Regular'),
(122, 1, 'J', 12, 'Regular'),
(123, 1, 'K', 1, 'Regular'),
(124, 1, 'K', 2, 'Regular'),
(125, 1, 'K', 3, 'Regular'),
(126, 1, 'K', 4, 'Regular'),
(127, 1, 'K', 5, 'Regular'),
(128, 1, 'K', 6, 'Regular'),
(129, 1, 'K', 7, 'Regular'),
(130, 1, 'K', 8, 'Regular'),
(131, 1, 'K', 9, 'Regular'),
(132, 1, 'K', 10, 'Regular'),
(133, 1, 'K', 11, 'Regular'),
(134, 1, 'K', 12, 'Regular'),
(135, 1, 'L', 1, 'Regular'),
(136, 1, 'L', 2, 'Regular'),
(137, 1, 'L', 3, 'Regular'),
(138, 1, 'L', 4, 'Regular'),
(139, 1, 'L', 5, 'Regular'),
(140, 1, 'L', 6, 'Regular'),
(141, 1, 'L', 7, 'Regular'),
(142, 1, 'L', 8, 'Regular'),
(143, 1, 'L', 9, 'Regular'),
(144, 1, 'L', 10, 'Regular'),
(145, 1, 'L', 11, 'Regular'),
(146, 1, 'L', 12, 'Regular'),
(258, 2, 'A', 1, 'PWD/Elderly'),
(259, 2, 'A', 2, 'PWD/Elderly'),
(260, 2, 'A', 3, 'PWD/Elderly'),
(261, 2, 'A', 4, 'PWD/Elderly'),
(262, 2, 'A', 5, 'PWD/Elderly'),
(263, 2, 'A', 6, 'PWD/Elderly'),
(264, 2, 'A', 7, 'PWD/Elderly'),
(265, 2, 'A', 8, 'PWD/Elderly'),
(266, 2, 'A', 9, 'PWD/Elderly'),
(267, 2, 'A', 10, 'PWD/Elderly'),
(268, 2, 'A', 11, 'PWD/Elderly'),
(269, 2, 'A', 12, 'PWD/Elderly'),
(270, 2, 'B', 1, 'PWD/Elderly'),
(271, 2, 'B', 2, 'PWD/Elderly'),
(272, 2, 'B', 3, 'PWD/Elderly'),
(273, 2, 'B', 4, 'PWD/Elderly'),
(274, 2, 'B', 5, 'PWD/Elderly'),
(275, 2, 'B', 6, 'PWD/Elderly'),
(276, 2, 'B', 7, 'PWD/Elderly'),
(277, 2, 'B', 8, 'PWD/Elderly'),
(278, 2, 'B', 9, 'PWD/Elderly'),
(279, 2, 'B', 10, 'PWD/Elderly'),
(280, 2, 'B', 11, 'PWD/Elderly'),
(281, 2, 'B', 12, 'PWD/Elderly'),
(282, 2, 'C', 1, 'Regular'),
(283, 2, 'C', 2, 'Regular'),
(284, 2, 'C', 3, 'Regular'),
(285, 2, 'C', 4, 'Regular'),
(286, 2, 'C', 5, 'Regular'),
(287, 2, 'C', 6, 'Regular'),
(288, 2, 'C', 7, 'Regular'),
(289, 2, 'C', 8, 'Regular'),
(290, 2, 'C', 9, 'Regular'),
(291, 2, 'C', 10, 'Regular'),
(292, 2, 'C', 11, 'Regular'),
(293, 2, 'C', 12, 'Regular'),
(294, 2, 'D', 1, 'Regular'),
(295, 2, 'D', 2, 'Regular'),
(296, 2, 'D', 3, 'Regular'),
(297, 2, 'D', 4, 'Regular'),
(298, 2, 'D', 5, 'Regular'),
(299, 2, 'D', 6, 'Regular'),
(300, 2, 'D', 7, 'Regular'),
(301, 2, 'D', 8, 'Regular'),
(302, 2, 'D', 9, 'Regular'),
(303, 2, 'D', 10, 'Regular'),
(304, 2, 'D', 11, 'Regular'),
(305, 2, 'D', 12, 'Regular'),
(306, 2, 'E', 1, 'Regular'),
(307, 2, 'E', 2, 'Regular'),
(308, 2, 'E', 3, 'Regular'),
(309, 2, 'E', 4, 'Regular'),
(310, 2, 'E', 5, 'Regular'),
(311, 2, 'E', 6, 'Regular'),
(312, 2, 'E', 7, 'Regular'),
(313, 2, 'E', 8, 'Regular'),
(314, 2, 'E', 9, 'Regular'),
(315, 2, 'E', 10, 'Regular'),
(316, 2, 'E', 11, 'Regular'),
(317, 2, 'E', 12, 'Regular'),
(318, 2, 'F', 1, 'Regular'),
(319, 2, 'F', 2, 'Regular'),
(320, 2, 'F', 3, 'Regular'),
(321, 2, 'F', 4, 'Regular'),
(322, 2, 'F', 5, 'Regular'),
(323, 2, 'F', 6, 'Regular'),
(324, 2, 'F', 7, 'Regular'),
(325, 2, 'F', 8, 'Regular'),
(326, 2, 'F', 9, 'Regular'),
(327, 2, 'F', 10, 'Regular'),
(328, 2, 'F', 11, 'Regular'),
(329, 2, 'F', 12, 'Regular'),
(330, 2, 'G', 1, 'Regular'),
(331, 2, 'G', 2, 'Regular'),
(332, 2, 'G', 3, 'Regular'),
(333, 2, 'G', 4, 'Regular'),
(334, 2, 'G', 5, 'Regular'),
(335, 2, 'G', 6, 'Regular'),
(336, 2, 'G', 7, 'Regular'),
(337, 2, 'G', 8, 'Regular'),
(338, 2, 'G', 9, 'Regular'),
(339, 2, 'G', 10, 'Regular'),
(340, 2, 'G', 11, 'Regular'),
(341, 2, 'G', 12, 'Regular'),
(342, 2, 'H', 1, 'Regular'),
(343, 2, 'H', 2, 'Regular'),
(344, 2, 'H', 3, 'Regular'),
(345, 2, 'H', 4, 'Regular'),
(346, 2, 'H', 5, 'Regular'),
(347, 2, 'H', 6, 'Regular'),
(348, 2, 'H', 7, 'Regular'),
(349, 2, 'H', 8, 'Regular'),
(350, 2, 'H', 9, 'Regular'),
(351, 2, 'H', 10, 'Regular'),
(352, 2, 'H', 11, 'Regular'),
(353, 2, 'H', 12, 'Regular'),
(354, 2, 'I', 1, 'Regular'),
(355, 2, 'I', 2, 'Regular'),
(356, 2, 'I', 3, 'Regular'),
(357, 2, 'I', 4, 'Regular'),
(358, 2, 'I', 5, 'Regular'),
(359, 2, 'I', 6, 'Regular'),
(360, 2, 'I', 7, 'Regular'),
(361, 2, 'I', 8, 'Regular'),
(362, 2, 'I', 9, 'Regular'),
(363, 2, 'I', 10, 'Regular'),
(364, 2, 'I', 11, 'Regular'),
(365, 2, 'I', 12, 'Regular'),
(366, 2, 'J', 1, 'Regular'),
(367, 2, 'J', 2, 'Regular'),
(368, 2, 'J', 3, 'Regular'),
(369, 2, 'J', 4, 'Regular'),
(370, 2, 'J', 5, 'Regular'),
(371, 2, 'J', 6, 'Regular'),
(372, 2, 'J', 7, 'Regular'),
(373, 2, 'J', 8, 'Regular'),
(374, 2, 'J', 9, 'Regular'),
(375, 2, 'J', 10, 'Regular'),
(376, 2, 'J', 11, 'Regular'),
(377, 2, 'J', 12, 'Regular'),
(378, 2, 'K', 1, 'Regular'),
(379, 2, 'K', 2, 'Regular'),
(380, 2, 'K', 3, 'Regular'),
(381, 2, 'K', 4, 'Regular'),
(382, 2, 'K', 5, 'Regular'),
(383, 2, 'K', 6, 'Regular'),
(384, 2, 'K', 7, 'Regular'),
(385, 2, 'K', 8, 'Regular'),
(386, 2, 'K', 9, 'Regular'),
(387, 2, 'K', 10, 'Regular'),
(388, 2, 'K', 11, 'Regular'),
(389, 2, 'K', 12, 'Regular'),
(390, 2, 'L', 1, 'Regular'),
(391, 2, 'L', 2, 'Regular'),
(392, 2, 'L', 3, 'Regular'),
(393, 2, 'L', 4, 'Regular'),
(394, 2, 'L', 5, 'Regular'),
(395, 2, 'L', 6, 'Regular'),
(396, 2, 'L', 7, 'Regular'),
(397, 2, 'L', 8, 'Regular'),
(398, 2, 'L', 9, 'Regular'),
(399, 2, 'L', 10, 'Regular'),
(400, 2, 'L', 11, 'Regular'),
(401, 2, 'L', 12, 'Regular'),
(402, 3, 'A', 1, 'PWD/Elderly'),
(403, 3, 'A', 2, 'PWD/Elderly'),
(404, 3, 'A', 3, 'PWD/Elderly'),
(405, 3, 'A', 4, 'PWD/Elderly'),
(406, 3, 'A', 5, 'PWD/Elderly'),
(407, 3, 'A', 6, 'PWD/Elderly'),
(408, 3, 'A', 7, 'PWD/Elderly'),
(409, 3, 'A', 8, 'PWD/Elderly'),
(410, 3, 'A', 9, 'PWD/Elderly'),
(411, 3, 'A', 10, 'PWD/Elderly'),
(412, 3, 'A', 11, 'PWD/Elderly'),
(413, 3, 'A', 12, 'PWD/Elderly'),
(414, 3, 'B', 1, 'PWD/Elderly'),
(415, 3, 'B', 2, 'PWD/Elderly'),
(416, 3, 'B', 3, 'PWD/Elderly'),
(417, 3, 'B', 4, 'PWD/Elderly'),
(418, 3, 'B', 5, 'PWD/Elderly'),
(419, 3, 'B', 6, 'PWD/Elderly'),
(420, 3, 'B', 7, 'PWD/Elderly'),
(421, 3, 'B', 8, 'PWD/Elderly'),
(422, 3, 'B', 9, 'PWD/Elderly'),
(423, 3, 'B', 10, 'PWD/Elderly'),
(424, 3, 'B', 11, 'PWD/Elderly'),
(425, 3, 'B', 12, 'PWD/Elderly'),
(426, 3, 'C', 1, 'Regular'),
(427, 3, 'C', 2, 'Regular'),
(428, 3, 'C', 3, 'Regular'),
(429, 3, 'C', 4, 'Regular'),
(430, 3, 'C', 5, 'Regular'),
(431, 3, 'C', 6, 'Regular'),
(432, 3, 'C', 7, 'Regular'),
(433, 3, 'C', 8, 'Regular'),
(434, 3, 'C', 9, 'Regular'),
(435, 3, 'C', 10, 'Regular'),
(436, 3, 'C', 11, 'Regular'),
(437, 3, 'C', 12, 'Regular'),
(438, 3, 'D', 1, 'Regular'),
(439, 3, 'D', 2, 'Regular'),
(440, 3, 'D', 3, 'Regular'),
(441, 3, 'D', 4, 'Regular'),
(442, 3, 'D', 5, 'Regular'),
(443, 3, 'D', 6, 'Regular'),
(444, 3, 'D', 7, 'Regular'),
(445, 3, 'D', 8, 'Regular'),
(446, 3, 'D', 9, 'Regular'),
(447, 3, 'D', 10, 'Regular'),
(448, 3, 'D', 11, 'Regular'),
(449, 3, 'D', 12, 'Regular'),
(450, 3, 'E', 1, 'Regular'),
(451, 3, 'E', 2, 'Regular'),
(452, 3, 'E', 3, 'Regular'),
(453, 3, 'E', 4, 'Regular'),
(454, 3, 'E', 5, 'Regular'),
(455, 3, 'E', 6, 'Regular'),
(456, 3, 'E', 7, 'Regular'),
(457, 3, 'E', 8, 'Regular'),
(458, 3, 'E', 9, 'Regular'),
(459, 3, 'E', 10, 'Regular'),
(460, 3, 'E', 11, 'Regular'),
(461, 3, 'E', 12, 'Regular'),
(462, 3, 'F', 1, 'Regular'),
(463, 3, 'F', 2, 'Regular'),
(464, 3, 'F', 3, 'Regular'),
(465, 3, 'F', 4, 'Regular'),
(466, 3, 'F', 5, 'Regular'),
(467, 3, 'F', 6, 'Regular'),
(468, 3, 'F', 7, 'Regular'),
(469, 3, 'F', 8, 'Regular'),
(470, 3, 'F', 9, 'Regular'),
(471, 3, 'F', 10, 'Regular'),
(472, 3, 'F', 11, 'Regular'),
(473, 3, 'F', 12, 'Regular'),
(474, 3, 'G', 1, 'Regular'),
(475, 3, 'G', 2, 'Regular'),
(476, 3, 'G', 3, 'Regular'),
(477, 3, 'G', 4, 'Regular'),
(478, 3, 'G', 5, 'Regular'),
(479, 3, 'G', 6, 'Regular'),
(480, 3, 'G', 7, 'Regular'),
(481, 3, 'G', 8, 'Regular'),
(482, 3, 'G', 9, 'Regular'),
(483, 3, 'G', 10, 'Regular'),
(484, 3, 'G', 11, 'Regular'),
(485, 3, 'G', 12, 'Regular'),
(486, 3, 'H', 1, 'Regular'),
(487, 3, 'H', 2, 'Regular'),
(488, 3, 'H', 3, 'Regular'),
(489, 3, 'H', 4, 'Regular'),
(490, 3, 'H', 5, 'Regular'),
(491, 3, 'H', 6, 'Regular'),
(492, 3, 'H', 7, 'Regular'),
(493, 3, 'H', 8, 'Regular'),
(494, 3, 'H', 9, 'Regular'),
(495, 3, 'H', 10, 'Regular'),
(496, 3, 'H', 11, 'Regular'),
(497, 3, 'H', 12, 'Regular'),
(498, 3, 'I', 1, 'Regular'),
(499, 3, 'I', 2, 'Regular'),
(500, 3, 'I', 3, 'Regular'),
(501, 3, 'I', 4, 'Regular'),
(502, 3, 'I', 5, 'Regular'),
(503, 3, 'I', 6, 'Regular'),
(504, 3, 'I', 7, 'Regular'),
(505, 3, 'I', 8, 'Regular'),
(506, 3, 'I', 9, 'Regular'),
(507, 3, 'I', 10, 'Regular'),
(508, 3, 'I', 11, 'Regular'),
(509, 3, 'I', 12, 'Regular'),
(510, 3, 'J', 1, 'Regular'),
(511, 3, 'J', 2, 'Regular'),
(512, 3, 'J', 3, 'Regular'),
(513, 3, 'J', 4, 'Regular'),
(514, 3, 'J', 5, 'Regular'),
(515, 3, 'J', 6, 'Regular'),
(516, 3, 'J', 7, 'Regular'),
(517, 3, 'J', 8, 'Regular'),
(518, 3, 'J', 9, 'Regular'),
(519, 3, 'J', 10, 'Regular'),
(520, 3, 'J', 11, 'Regular'),
(521, 3, 'J', 12, 'Regular'),
(522, 3, 'K', 1, 'Regular'),
(523, 3, 'K', 2, 'Regular'),
(524, 3, 'K', 3, 'Regular'),
(525, 3, 'K', 4, 'Regular'),
(526, 3, 'K', 5, 'Regular'),
(527, 3, 'K', 6, 'Regular'),
(528, 3, 'K', 7, 'Regular'),
(529, 3, 'K', 8, 'Regular'),
(530, 3, 'K', 9, 'Regular'),
(531, 3, 'K', 10, 'Regular'),
(532, 3, 'K', 11, 'Regular'),
(533, 3, 'K', 12, 'Regular'),
(534, 3, 'L', 1, 'Regular'),
(535, 3, 'L', 2, 'Regular'),
(536, 3, 'L', 3, 'Regular'),
(537, 3, 'L', 4, 'Regular'),
(538, 3, 'L', 5, 'Regular'),
(539, 3, 'L', 6, 'Regular'),
(540, 3, 'L', 7, 'Regular'),
(541, 3, 'L', 8, 'Regular'),
(542, 3, 'L', 9, 'Regular'),
(543, 3, 'L', 10, 'Regular'),
(544, 3, 'L', 11, 'Regular'),
(545, 3, 'L', 12, 'Regular'),
(546, 4, 'A', 1, 'PWD/Elderly'),
(547, 4, 'A', 2, 'PWD/Elderly'),
(548, 4, 'A', 3, 'PWD/Elderly'),
(549, 4, 'A', 4, 'PWD/Elderly'),
(550, 4, 'A', 5, 'PWD/Elderly'),
(551, 4, 'A', 6, 'PWD/Elderly'),
(552, 4, 'A', 7, 'PWD/Elderly'),
(553, 4, 'A', 8, 'PWD/Elderly'),
(554, 4, 'A', 9, 'PWD/Elderly'),
(555, 4, 'A', 10, 'PWD/Elderly'),
(556, 4, 'A', 11, 'PWD/Elderly'),
(557, 4, 'A', 12, 'PWD/Elderly'),
(558, 4, 'B', 1, 'PWD/Elderly'),
(559, 4, 'B', 2, 'PWD/Elderly'),
(560, 4, 'B', 3, 'PWD/Elderly'),
(561, 4, 'B', 4, 'PWD/Elderly'),
(562, 4, 'B', 5, 'PWD/Elderly'),
(563, 4, 'B', 6, 'PWD/Elderly'),
(564, 4, 'B', 7, 'PWD/Elderly'),
(565, 4, 'B', 8, 'PWD/Elderly'),
(566, 4, 'B', 9, 'PWD/Elderly'),
(567, 4, 'B', 10, 'PWD/Elderly'),
(568, 4, 'B', 11, 'PWD/Elderly'),
(569, 4, 'B', 12, 'PWD/Elderly'),
(570, 4, 'C', 1, 'Regular'),
(571, 4, 'C', 2, 'Regular'),
(572, 4, 'C', 3, 'Regular'),
(573, 4, 'C', 4, 'Regular'),
(574, 4, 'C', 5, 'Regular'),
(575, 4, 'C', 6, 'Regular'),
(576, 4, 'C', 7, 'Regular'),
(577, 4, 'C', 8, 'Regular'),
(578, 4, 'C', 9, 'Regular'),
(579, 4, 'C', 10, 'Regular'),
(580, 4, 'C', 11, 'Regular'),
(581, 4, 'C', 12, 'Regular'),
(582, 4, 'D', 1, 'Regular'),
(583, 4, 'D', 2, 'Regular'),
(584, 4, 'D', 3, 'Regular'),
(585, 4, 'D', 4, 'Regular'),
(586, 4, 'D', 5, 'Regular'),
(587, 4, 'D', 6, 'Regular'),
(588, 4, 'D', 7, 'Regular'),
(589, 4, 'D', 8, 'Regular'),
(590, 4, 'D', 9, 'Regular'),
(591, 4, 'D', 10, 'Regular'),
(592, 4, 'D', 11, 'Regular'),
(593, 4, 'D', 12, 'Regular'),
(594, 4, 'E', 1, 'Regular'),
(595, 4, 'E', 2, 'Regular'),
(596, 4, 'E', 3, 'Regular'),
(597, 4, 'E', 4, 'Regular'),
(598, 4, 'E', 5, 'Regular'),
(599, 4, 'E', 6, 'Regular'),
(600, 4, 'E', 7, 'Regular'),
(601, 4, 'E', 8, 'Regular'),
(602, 4, 'E', 9, 'Regular'),
(603, 4, 'E', 10, 'Regular'),
(604, 4, 'E', 11, 'Regular'),
(605, 4, 'E', 12, 'Regular'),
(606, 4, 'F', 1, 'Regular'),
(607, 4, 'F', 2, 'Regular'),
(608, 4, 'F', 3, 'Regular'),
(609, 4, 'F', 4, 'Regular'),
(610, 4, 'F', 5, 'Regular'),
(611, 4, 'F', 6, 'Regular'),
(612, 4, 'F', 7, 'Regular'),
(613, 4, 'F', 8, 'Regular'),
(614, 4, 'F', 9, 'Regular'),
(615, 4, 'F', 10, 'Regular'),
(616, 4, 'F', 11, 'Regular'),
(617, 4, 'F', 12, 'Regular'),
(618, 4, 'G', 1, 'Regular'),
(619, 4, 'G', 2, 'Regular'),
(620, 4, 'G', 3, 'Regular'),
(621, 4, 'G', 4, 'Regular'),
(622, 4, 'G', 5, 'Regular'),
(623, 4, 'G', 6, 'Regular'),
(624, 4, 'G', 7, 'Regular'),
(625, 4, 'G', 8, 'Regular'),
(626, 4, 'G', 9, 'Regular'),
(627, 4, 'G', 10, 'Regular'),
(628, 4, 'G', 11, 'Regular'),
(629, 4, 'G', 12, 'Regular'),
(630, 4, 'H', 1, 'Regular'),
(631, 4, 'H', 2, 'Regular'),
(632, 4, 'H', 3, 'Regular'),
(633, 4, 'H', 4, 'Regular'),
(634, 4, 'H', 5, 'Regular'),
(635, 4, 'H', 6, 'Regular'),
(636, 4, 'H', 7, 'Regular'),
(637, 4, 'H', 8, 'Regular'),
(638, 4, 'H', 9, 'Regular'),
(639, 4, 'H', 10, 'Regular'),
(640, 4, 'H', 11, 'Regular'),
(641, 4, 'H', 12, 'Regular'),
(642, 4, 'I', 1, 'Regular'),
(643, 4, 'I', 2, 'Regular'),
(644, 4, 'I', 3, 'Regular'),
(645, 4, 'I', 4, 'Regular'),
(646, 4, 'I', 5, 'Regular'),
(647, 4, 'I', 6, 'Regular'),
(648, 4, 'I', 7, 'Regular'),
(649, 4, 'I', 8, 'Regular'),
(650, 4, 'I', 9, 'Regular'),
(651, 4, 'I', 10, 'Regular'),
(652, 4, 'I', 11, 'Regular'),
(653, 4, 'I', 12, 'Regular'),
(654, 4, 'J', 1, 'Regular'),
(655, 4, 'J', 2, 'Regular'),
(656, 4, 'J', 3, 'Regular'),
(657, 4, 'J', 4, 'Regular'),
(658, 4, 'J', 5, 'Regular'),
(659, 4, 'J', 6, 'Regular'),
(660, 4, 'J', 7, 'Regular'),
(661, 4, 'J', 8, 'Regular'),
(662, 4, 'J', 9, 'Regular'),
(663, 4, 'J', 10, 'Regular'),
(664, 4, 'J', 11, 'Regular'),
(665, 4, 'J', 12, 'Regular'),
(666, 4, 'K', 1, 'Regular'),
(667, 4, 'K', 2, 'Regular'),
(668, 4, 'K', 3, 'Regular'),
(669, 4, 'K', 4, 'Regular'),
(670, 4, 'K', 5, 'Regular'),
(671, 4, 'K', 6, 'Regular'),
(672, 4, 'K', 7, 'Regular'),
(673, 4, 'K', 8, 'Regular'),
(674, 4, 'K', 9, 'Regular'),
(675, 4, 'K', 10, 'Regular'),
(676, 4, 'K', 11, 'Regular'),
(677, 4, 'K', 12, 'Regular'),
(678, 4, 'L', 1, 'Regular'),
(679, 4, 'L', 2, 'Regular'),
(680, 4, 'L', 3, 'Regular'),
(681, 4, 'L', 4, 'Regular'),
(682, 4, 'L', 5, 'Regular'),
(683, 4, 'L', 6, 'Regular'),
(684, 4, 'L', 7, 'Regular'),
(685, 4, 'L', 8, 'Regular'),
(686, 4, 'L', 9, 'Regular'),
(687, 4, 'L', 10, 'Regular'),
(688, 4, 'L', 11, 'Regular'),
(689, 4, 'L', 12, 'Regular'),
(769, 5, 'A', 1, 'PWD/Elderly'),
(770, 5, 'B', 1, 'PWD/Elderly'),
(771, 5, 'C', 1, 'Regular'),
(772, 5, 'D', 1, 'Regular'),
(773, 5, 'E', 1, 'Regular'),
(774, 5, 'F', 1, 'Regular'),
(775, 6, 'A', 1, 'PWD/Elderly'),
(776, 6, 'B', 1, 'PWD/Elderly'),
(777, 6, 'C', 1, 'Regular'),
(778, 6, 'D', 1, 'Regular'),
(779, 6, 'E', 1, 'Regular'),
(780, 6, 'F', 1, 'Regular'),
(781, 7, 'A', 1, 'PWD/Elderly'),
(782, 7, 'B', 1, 'PWD/Elderly'),
(783, 7, 'C', 1, 'Regular'),
(784, 7, 'D', 1, 'Regular'),
(785, 7, 'E', 1, 'Regular'),
(786, 7, 'F', 1, 'Regular'),
(787, 8, 'A', 1, 'PWD/Elderly'),
(788, 8, 'B', 1, 'PWD/Elderly'),
(789, 8, 'C', 1, 'Regular'),
(790, 8, 'D', 1, 'Regular'),
(791, 8, 'E', 1, 'Regular'),
(792, 8, 'F', 1, 'Regular'),
(793, 5, 'A', 2, 'PWD/Elderly'),
(794, 5, 'B', 2, 'PWD/Elderly'),
(795, 5, 'C', 2, 'Regular'),
(796, 5, 'D', 2, 'Regular'),
(797, 5, 'E', 2, 'Regular'),
(798, 5, 'F', 2, 'Regular'),
(799, 6, 'A', 2, 'PWD/Elderly'),
(800, 6, 'B', 2, 'PWD/Elderly'),
(801, 6, 'C', 2, 'Regular'),
(802, 6, 'D', 2, 'Regular'),
(803, 6, 'E', 2, 'Regular'),
(804, 6, 'F', 2, 'Regular'),
(805, 7, 'A', 2, 'PWD/Elderly'),
(806, 7, 'B', 2, 'PWD/Elderly'),
(807, 7, 'C', 2, 'Regular'),
(808, 7, 'D', 2, 'Regular'),
(809, 7, 'E', 2, 'Regular'),
(810, 7, 'F', 2, 'Regular'),
(811, 8, 'A', 2, 'PWD/Elderly'),
(812, 8, 'B', 2, 'PWD/Elderly'),
(813, 8, 'C', 2, 'Regular'),
(814, 8, 'D', 2, 'Regular'),
(815, 8, 'E', 2, 'Regular'),
(816, 8, 'F', 2, 'Regular'),
(817, 5, 'A', 3, 'PWD/Elderly'),
(818, 5, 'B', 3, 'PWD/Elderly'),
(819, 5, 'C', 3, 'Regular'),
(820, 5, 'D', 3, 'Regular'),
(821, 5, 'E', 3, 'Regular'),
(822, 5, 'F', 3, 'Regular'),
(823, 6, 'A', 3, 'PWD/Elderly'),
(824, 6, 'B', 3, 'PWD/Elderly'),
(825, 6, 'C', 3, 'Regular'),
(826, 6, 'D', 3, 'Regular'),
(827, 6, 'E', 3, 'Regular'),
(828, 6, 'F', 3, 'Regular'),
(829, 7, 'A', 3, 'PWD/Elderly'),
(830, 7, 'B', 3, 'PWD/Elderly'),
(831, 7, 'C', 3, 'Regular'),
(832, 7, 'D', 3, 'Regular'),
(833, 7, 'E', 3, 'Regular'),
(834, 7, 'F', 3, 'Regular'),
(835, 8, 'A', 3, 'PWD/Elderly'),
(836, 8, 'B', 3, 'PWD/Elderly'),
(837, 8, 'C', 3, 'Regular'),
(838, 8, 'D', 3, 'Regular'),
(839, 8, 'E', 3, 'Regular'),
(840, 8, 'F', 3, 'Regular'),
(841, 5, 'A', 4, 'PWD/Elderly'),
(842, 5, 'B', 4, 'PWD/Elderly'),
(843, 5, 'C', 4, 'Regular'),
(844, 5, 'D', 4, 'Regular'),
(845, 5, 'E', 4, 'Regular'),
(846, 5, 'F', 4, 'Regular'),
(847, 6, 'A', 4, 'PWD/Elderly'),
(848, 6, 'B', 4, 'PWD/Elderly'),
(849, 6, 'C', 4, 'Regular'),
(850, 6, 'D', 4, 'Regular'),
(851, 6, 'E', 4, 'Regular'),
(852, 6, 'F', 4, 'Regular'),
(853, 7, 'A', 4, 'PWD/Elderly'),
(854, 7, 'B', 4, 'PWD/Elderly'),
(855, 7, 'C', 4, 'Regular'),
(856, 7, 'D', 4, 'Regular'),
(857, 7, 'E', 4, 'Regular'),
(858, 7, 'F', 4, 'Regular'),
(859, 8, 'A', 4, 'PWD/Elderly'),
(860, 8, 'B', 4, 'PWD/Elderly'),
(861, 8, 'C', 4, 'Regular'),
(862, 8, 'D', 4, 'Regular'),
(863, 8, 'E', 4, 'Regular'),
(864, 8, 'F', 4, 'Regular'),
(865, 5, 'A', 5, 'PWD/Elderly'),
(866, 5, 'B', 5, 'PWD/Elderly'),
(867, 5, 'C', 5, 'Regular'),
(868, 5, 'D', 5, 'Regular'),
(869, 5, 'E', 5, 'Regular'),
(870, 5, 'F', 5, 'Regular'),
(871, 6, 'A', 5, 'PWD/Elderly'),
(872, 6, 'B', 5, 'PWD/Elderly'),
(873, 6, 'C', 5, 'Regular'),
(874, 6, 'D', 5, 'Regular'),
(875, 6, 'E', 5, 'Regular'),
(876, 6, 'F', 5, 'Regular'),
(877, 7, 'A', 5, 'PWD/Elderly'),
(878, 7, 'B', 5, 'PWD/Elderly'),
(879, 7, 'C', 5, 'Regular'),
(880, 7, 'D', 5, 'Regular'),
(881, 7, 'E', 5, 'Regular'),
(882, 7, 'F', 5, 'Regular'),
(883, 8, 'A', 5, 'PWD/Elderly'),
(884, 8, 'B', 5, 'PWD/Elderly'),
(885, 8, 'C', 5, 'Regular'),
(886, 8, 'D', 5, 'Regular'),
(887, 8, 'E', 5, 'Regular'),
(888, 8, 'F', 5, 'Regular'),
(889, 5, 'A', 6, 'PWD/Elderly'),
(890, 5, 'B', 6, 'PWD/Elderly'),
(891, 5, 'C', 6, 'Regular'),
(892, 5, 'D', 6, 'Regular'),
(893, 5, 'E', 6, 'Regular'),
(894, 5, 'F', 6, 'Regular'),
(895, 6, 'A', 6, 'PWD/Elderly'),
(896, 6, 'B', 6, 'PWD/Elderly'),
(897, 6, 'C', 6, 'Regular'),
(898, 6, 'D', 6, 'Regular'),
(899, 6, 'E', 6, 'Regular'),
(900, 6, 'F', 6, 'Regular'),
(901, 7, 'A', 6, 'PWD/Elderly'),
(902, 7, 'B', 6, 'PWD/Elderly'),
(903, 7, 'C', 6, 'Regular'),
(904, 7, 'D', 6, 'Regular'),
(905, 7, 'E', 6, 'Regular'),
(906, 7, 'F', 6, 'Regular'),
(907, 8, 'A', 6, 'PWD/Elderly'),
(908, 8, 'B', 6, 'PWD/Elderly'),
(909, 8, 'C', 6, 'Regular'),
(910, 8, 'D', 6, 'Regular'),
(911, 8, 'E', 6, 'Regular'),
(912, 8, 'F', 6, 'Regular'),
(913, 5, 'A', 7, 'PWD/Elderly'),
(914, 5, 'B', 7, 'PWD/Elderly'),
(915, 5, 'C', 7, 'Regular'),
(916, 5, 'D', 7, 'Regular'),
(917, 5, 'E', 7, 'Regular'),
(918, 5, 'F', 7, 'Regular'),
(919, 6, 'A', 7, 'PWD/Elderly'),
(920, 6, 'B', 7, 'PWD/Elderly'),
(921, 6, 'C', 7, 'Regular'),
(922, 6, 'D', 7, 'Regular'),
(923, 6, 'E', 7, 'Regular'),
(924, 6, 'F', 7, 'Regular'),
(925, 7, 'A', 7, 'PWD/Elderly'),
(926, 7, 'B', 7, 'PWD/Elderly'),
(927, 7, 'C', 7, 'Regular'),
(928, 7, 'D', 7, 'Regular'),
(929, 7, 'E', 7, 'Regular'),
(930, 7, 'F', 7, 'Regular'),
(931, 8, 'A', 7, 'PWD/Elderly'),
(932, 8, 'B', 7, 'PWD/Elderly'),
(933, 8, 'C', 7, 'Regular'),
(934, 8, 'D', 7, 'Regular'),
(935, 8, 'E', 7, 'Regular'),
(936, 8, 'F', 7, 'Regular'),
(937, 5, 'A', 8, 'PWD/Elderly'),
(938, 5, 'B', 8, 'PWD/Elderly'),
(939, 5, 'C', 8, 'Regular'),
(940, 5, 'D', 8, 'Regular'),
(941, 5, 'E', 8, 'Regular'),
(942, 5, 'F', 8, 'Regular'),
(943, 6, 'A', 8, 'PWD/Elderly'),
(944, 6, 'B', 8, 'PWD/Elderly'),
(945, 6, 'C', 8, 'Regular'),
(946, 6, 'D', 8, 'Regular'),
(947, 6, 'E', 8, 'Regular'),
(948, 6, 'F', 8, 'Regular'),
(949, 7, 'A', 8, 'PWD/Elderly'),
(950, 7, 'B', 8, 'PWD/Elderly'),
(951, 7, 'C', 8, 'Regular'),
(952, 7, 'D', 8, 'Regular'),
(953, 7, 'E', 8, 'Regular'),
(954, 7, 'F', 8, 'Regular'),
(955, 8, 'A', 8, 'PWD/Elderly'),
(956, 8, 'B', 8, 'PWD/Elderly'),
(957, 8, 'C', 8, 'Regular'),
(958, 8, 'D', 8, 'Regular'),
(959, 8, 'E', 8, 'Regular'),
(960, 8, 'F', 8, 'Regular'),
(961, 5, 'A', 9, 'PWD/Elderly'),
(962, 5, 'B', 9, 'PWD/Elderly'),
(963, 5, 'C', 9, 'Regular'),
(964, 5, 'D', 9, 'Regular'),
(965, 5, 'E', 9, 'Regular'),
(966, 5, 'F', 9, 'Regular'),
(967, 6, 'A', 9, 'PWD/Elderly'),
(968, 6, 'B', 9, 'PWD/Elderly'),
(969, 6, 'C', 9, 'Regular'),
(970, 6, 'D', 9, 'Regular'),
(971, 6, 'E', 9, 'Regular'),
(972, 6, 'F', 9, 'Regular'),
(973, 7, 'A', 9, 'PWD/Elderly'),
(974, 7, 'B', 9, 'PWD/Elderly'),
(975, 7, 'C', 9, 'Regular'),
(976, 7, 'D', 9, 'Regular'),
(977, 7, 'E', 9, 'Regular'),
(978, 7, 'F', 9, 'Regular'),
(979, 8, 'A', 9, 'PWD/Elderly'),
(980, 8, 'B', 9, 'PWD/Elderly'),
(981, 8, 'C', 9, 'Regular'),
(982, 8, 'D', 9, 'Regular'),
(983, 8, 'E', 9, 'Regular'),
(984, 8, 'F', 9, 'Regular'),
(985, 5, 'A', 10, 'PWD/Elderly'),
(986, 5, 'B', 10, 'PWD/Elderly'),
(987, 5, 'C', 10, 'Regular'),
(988, 5, 'D', 10, 'Regular'),
(989, 5, 'E', 10, 'Regular'),
(990, 5, 'F', 10, 'Regular'),
(991, 6, 'A', 10, 'PWD/Elderly'),
(992, 6, 'B', 10, 'PWD/Elderly'),
(993, 6, 'C', 10, 'Regular'),
(994, 6, 'D', 10, 'Regular'),
(995, 6, 'E', 10, 'Regular'),
(996, 6, 'F', 10, 'Regular'),
(997, 7, 'A', 10, 'PWD/Elderly'),
(998, 7, 'B', 10, 'PWD/Elderly'),
(999, 7, 'C', 10, 'Regular'),
(1000, 7, 'D', 10, 'Regular'),
(1001, 7, 'E', 10, 'Regular'),
(1002, 7, 'F', 10, 'Regular'),
(1003, 8, 'A', 10, 'PWD/Elderly'),
(1004, 8, 'B', 10, 'PWD/Elderly'),
(1005, 8, 'C', 10, 'Regular'),
(1006, 8, 'D', 10, 'Regular'),
(1007, 8, 'E', 10, 'Regular'),
(1008, 8, 'F', 10, 'Regular'),
(1009, 5, 'A', 11, 'PWD/Elderly'),
(1010, 5, 'B', 11, 'PWD/Elderly'),
(1011, 5, 'C', 11, 'Regular'),
(1012, 5, 'D', 11, 'Regular'),
(1013, 5, 'E', 11, 'Regular'),
(1014, 5, 'F', 11, 'Regular'),
(1015, 6, 'A', 11, 'PWD/Elderly'),
(1016, 6, 'B', 11, 'PWD/Elderly'),
(1017, 6, 'C', 11, 'Regular'),
(1018, 6, 'D', 11, 'Regular'),
(1019, 6, 'E', 11, 'Regular'),
(1020, 6, 'F', 11, 'Regular'),
(1021, 7, 'A', 11, 'PWD/Elderly'),
(1022, 7, 'B', 11, 'PWD/Elderly'),
(1023, 7, 'C', 11, 'Regular'),
(1024, 7, 'D', 11, 'Regular'),
(1025, 7, 'E', 11, 'Regular'),
(1026, 7, 'F', 11, 'Regular'),
(1027, 8, 'A', 11, 'PWD/Elderly'),
(1028, 8, 'B', 11, 'PWD/Elderly'),
(1029, 8, 'C', 11, 'Regular'),
(1030, 8, 'D', 11, 'Regular'),
(1031, 8, 'E', 11, 'Regular'),
(1032, 8, 'F', 11, 'Regular'),
(1033, 5, 'A', 12, 'PWD/Elderly'),
(1034, 5, 'B', 12, 'PWD/Elderly'),
(1035, 5, 'C', 12, 'Regular'),
(1036, 5, 'D', 12, 'Regular'),
(1037, 5, 'E', 12, 'Regular'),
(1038, 5, 'F', 12, 'Regular'),
(1039, 6, 'A', 12, 'PWD/Elderly'),
(1040, 6, 'B', 12, 'PWD/Elderly'),
(1041, 6, 'C', 12, 'Regular'),
(1042, 6, 'D', 12, 'Regular'),
(1043, 6, 'E', 12, 'Regular'),
(1044, 6, 'F', 12, 'Regular'),
(1045, 7, 'A', 12, 'PWD/Elderly'),
(1046, 7, 'B', 12, 'PWD/Elderly'),
(1047, 7, 'C', 12, 'Regular'),
(1048, 7, 'D', 12, 'Regular'),
(1049, 7, 'E', 12, 'Regular'),
(1050, 7, 'F', 12, 'Regular'),
(1051, 8, 'A', 12, 'PWD/Elderly'),
(1052, 8, 'B', 12, 'PWD/Elderly'),
(1053, 8, 'C', 12, 'Regular'),
(1054, 8, 'D', 12, 'Regular'),
(1055, 8, 'E', 12, 'Regular'),
(1056, 8, 'F', 12, 'Regular');

-- --------------------------------------------------------

--
-- Table structure for table `showtimes`
--

CREATE TABLE `showtimes` (
  `showtime_id` int(11) NOT NULL,
  `movie_id` int(11) NOT NULL,
  `screen_id` int(11) NOT NULL,
  `start_time` datetime NOT NULL,
  `price` decimal(8,2) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `showtimes`
--

INSERT INTO `showtimes` (`showtime_id`, `movie_id`, `screen_id`, `start_time`, `price`) VALUES
(5, 1, 2, '2026-10-24 10:30:00', 180.00),
(6, 1, 3, '2026-10-24 14:00:00', 200.00),
(7, 1, 4, '2026-10-24 18:30:00', 220.00),
(8, 1, 5, '2026-10-25 12:00:00', 180.00),
(9, 1, 6, '2026-10-25 19:00:00', 220.00),
(10, 2, 3, '2026-10-24 10:30:00', 180.00),
(11, 2, 4, '2026-10-24 14:00:00', 200.00),
(12, 2, 5, '2026-10-24 18:30:00', 220.00),
(13, 2, 6, '2026-10-25 12:00:00', 180.00),
(14, 2, 7, '2026-10-25 19:00:00', 220.00),
(15, 3, 4, '2026-10-24 10:30:00', 180.00),
(16, 3, 5, '2026-10-24 14:00:00', 200.00),
(17, 3, 6, '2026-10-24 18:30:00', 220.00),
(18, 3, 7, '2026-10-25 12:00:00', 180.00),
(19, 3, 8, '2026-10-25 19:00:00', 220.00),
(20, 4, 5, '2026-10-24 10:30:00', 180.00),
(21, 4, 6, '2026-10-24 14:00:00', 200.00),
(22, 4, 7, '2026-10-24 18:30:00', 220.00),
(23, 4, 8, '2026-10-25 12:00:00', 180.00),
(24, 4, 1, '2026-10-25 19:00:00', 220.00),
(25, 5, 6, '2026-10-24 10:30:00', 180.00),
(26, 5, 7, '2026-10-24 14:00:00', 200.00),
(27, 5, 8, '2026-10-24 18:30:00', 220.00),
(28, 5, 1, '2026-10-25 12:00:00', 180.00),
(29, 5, 2, '2026-10-25 19:00:00', 220.00),
(30, 6, 7, '2026-10-24 10:30:00', 180.00),
(31, 6, 8, '2026-10-24 14:00:00', 200.00),
(32, 6, 1, '2026-10-24 18:30:00', 220.00),
(33, 6, 2, '2026-10-25 12:00:00', 180.00),
(34, 6, 3, '2026-10-25 19:00:00', 220.00),
(35, 7, 8, '2026-10-24 10:30:00', 180.00),
(36, 7, 1, '2026-10-24 14:00:00', 200.00),
(37, 7, 2, '2026-10-24 18:30:00', 220.00),
(38, 7, 3, '2026-10-25 12:00:00', 180.00),
(39, 7, 4, '2026-10-25 19:00:00', 220.00),
(40, 8, 1, '2026-10-24 10:30:00', 180.00),
(41, 8, 2, '2026-10-24 14:00:00', 200.00),
(42, 8, 3, '2026-10-24 18:30:00', 220.00),
(43, 8, 4, '2026-10-25 12:00:00', 180.00),
(44, 8, 5, '2026-10-25 19:00:00', 220.00),
(45, 9, 2, '2026-10-24 10:30:00', 180.00),
(46, 9, 3, '2026-10-24 14:00:00', 200.00),
(47, 9, 4, '2026-10-24 18:30:00', 220.00),
(48, 9, 5, '2026-10-25 12:00:00', 180.00),
(49, 9, 6, '2026-10-25 19:00:00', 220.00),
(50, 10, 3, '2026-10-24 10:30:00', 180.00),
(51, 10, 4, '2026-10-24 14:00:00', 200.00),
(52, 10, 5, '2026-10-24 18:30:00', 220.00),
(53, 10, 6, '2026-10-25 12:00:00', 180.00),
(54, 10, 7, '2026-10-25 19:00:00', 220.00);

--
-- Indexes for dumped tables
--

--
-- Indexes for table `accounts`
--
ALTER TABLE `accounts`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `bookings`
--
ALTER TABLE `bookings`
  ADD PRIMARY KEY (`booking_id`);

--
-- Indexes for table `booking_seats`
--
ALTER TABLE `booking_seats`
  ADD PRIMARY KEY (`showtime_id`,`seat_id`),
  ADD KEY `booking_id` (`booking_id`),
  ADD KEY `seat_id` (`seat_id`);

--
-- Indexes for table `screens`
--
ALTER TABLE `screens`
  ADD PRIMARY KEY (`screen_id`);

--
-- Indexes for table `seats`
--
ALTER TABLE `seats`
  ADD PRIMARY KEY (`seat_id`),
  ADD UNIQUE KEY `screen_id` (`screen_id`,`row_label`,`seat_number`),
  ADD UNIQUE KEY `uq_seat` (`screen_id`,`row_label`,`seat_number`);

--
-- Indexes for table `showtimes`
--
ALTER TABLE `showtimes`
  ADD PRIMARY KEY (`showtime_id`),
  ADD KEY `movie_id` (`movie_id`),
  ADD KEY `screen_id` (`screen_id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `accounts`
--
ALTER TABLE `accounts`
  MODIFY `id` int(255) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `bookings`
--
ALTER TABLE `bookings`
  MODIFY `booking_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `screens`
--
ALTER TABLE `screens`
  MODIFY `screen_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT for table `seats`
--
ALTER TABLE `seats`
  MODIFY `seat_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=1057;

--
-- AUTO_INCREMENT for table `showtimes`
--
ALTER TABLE `showtimes`
  MODIFY `showtime_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=55;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `booking_seats`
--
ALTER TABLE `booking_seats`
  ADD CONSTRAINT `booking_seats_ibfk_1` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`booking_id`),
  ADD CONSTRAINT `booking_seats_ibfk_2` FOREIGN KEY (`showtime_id`) REFERENCES `showtimes` (`showtime_id`),
  ADD CONSTRAINT `booking_seats_ibfk_3` FOREIGN KEY (`seat_id`) REFERENCES `seats` (`seat_id`);

--
-- Constraints for table `seats`
--
ALTER TABLE `seats`
  ADD CONSTRAINT `seats_ibfk_1` FOREIGN KEY (`screen_id`) REFERENCES `screens` (`screen_id`);

--
-- Constraints for table `showtimes`
--
ALTER TABLE `showtimes`
  ADD CONSTRAINT `showtimes_ibfk_2` FOREIGN KEY (`screen_id`) REFERENCES `screens` (`screen_id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;

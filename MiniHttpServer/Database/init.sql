-- Удаляем таблицы если существуют (для повторного запуска)
DROP TABLE IF EXISTS bookings CASCADE;
DROP TABLE IF EXISTS tour_images CASCADE;
DROP TABLE IF EXISTS tour_dates CASCADE;
DROP TABLE IF EXISTS tours CASCADE;
DROP TABLE IF EXISTS cities CASCADE;
DROP TABLE IF EXISTS countries CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Таблица стран
CREATE TABLE countries (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица городов назначения
CREATE TABLE cities (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    country_id INT REFERENCES countries(id) ON DELETE CASCADE,
    description TEXT,
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица городов отправления (в Германии)
CREATE TABLE departure_cities (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица туров
CREATE TABLE tours (
    id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    short_description TEXT,
    full_description TEXT,
    additional_description TEXT,
    included_in_price TEXT,
    not_included_in_price TEXT,
    base_price DECIMAL(10, 2) NOT NULL,
    duration INT NOT NULL DEFAULT 1,
    tour_type VARCHAR(50) DEFAULT 'bus',
    city_id INT REFERENCES cities(id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Связь туров с городами отправления
CREATE TABLE tour_departure_cities (
    id SERIAL PRIMARY KEY,
    tour_id INT REFERENCES tours(id) ON DELETE CASCADE,
    departure_city_id INT REFERENCES departure_cities(id) ON DELETE CASCADE,
    UNIQUE(tour_id, departure_city_id)
);

-- Таблица дат туров
CREATE TABLE tour_dates (
    id SERIAL PRIMARY KEY,
    tour_id INT REFERENCES tours(id) ON DELETE CASCADE,
    departure_date DATE NOT NULL,
    return_date DATE NOT NULL,
    available_seats INT DEFAULT 50,
    status VARCHAR(20) DEFAULT 'available',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица изображений туров
CREATE TABLE tour_images (
    id SERIAL PRIMARY KEY,
    tour_id INT REFERENCES tours(id) ON DELETE CASCADE,
    image_url VARCHAR(500) NOT NULL,
    is_cover BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица пользователей
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) DEFAULT 'user',
    full_name VARCHAR(255),
    phone VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Таблица бронирований
CREATE TABLE bookings (
    id SERIAL PRIMARY KEY,
    tour_id INT REFERENCES tours(id) ON DELETE CASCADE,
    user_id INT REFERENCES users(id) ON DELETE SET NULL,
    full_name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    departure_date DATE NOT NULL,
    adults_count INT DEFAULT 1,
    children_count INT DEFAULT 0,
    total_price DECIMAL(10, 2) NOT NULL,
    comment TEXT,
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Создаем индексы для быстрого поиска
CREATE INDEX idx_tours_city ON tours(city_id);
CREATE INDEX idx_cities_country ON cities(country_id);
CREATE INDEX idx_tour_dates_tour ON tour_dates(tour_id);
CREATE INDEX idx_tour_images_tour ON tour_images(tour_id);
CREATE INDEX idx_bookings_tour ON bookings(tour_id);
CREATE INDEX idx_bookings_user ON bookings(user_id);
CREATE INDEX idx_bookings_email ON bookings(email);
CREATE INDEX idx_tour_departure_cities_tour ON tour_departure_cities(tour_id);

-- Комментарии к таблицам
COMMENT ON TABLE countries IS 'Справочник стран';
COMMENT ON TABLE cities IS 'Справочник городов';
COMMENT ON TABLE tours IS 'Каталог туров';
COMMENT ON TABLE tour_dates IS 'Доступные даты для туров';
COMMENT ON TABLE tour_images IS 'Изображения туров';
COMMENT ON TABLE users IS 'Пользователи системы';
COMMENT ON TABLE bookings IS 'Бронирования туров';

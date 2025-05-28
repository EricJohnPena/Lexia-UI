-- Create profile pictures table
CREATE TABLE IF NOT EXISTS profile_pictures (
    picture_id INT AUTO_INCREMENT PRIMARY KEY,
    image_path VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Add profile picture ID column to students table
ALTER TABLE students_tbl
ADD COLUMN fk_profile_picture_id INT,
ADD CONSTRAINT fk_profile_picture
FOREIGN KEY (fk_profile_picture_id) REFERENCES profile_pictures(picture_id);

-- Insert default profile picture
INSERT INTO profile_pictures (image_path) VALUES ('default_profile.png'); 
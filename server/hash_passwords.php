<?php
include 'db_connection.php';

try {
    // First, let's check the table structure
    $check_query = "DESCRIBE students_tbl";
    $check_result = $conn->query($check_query);
    echo "Table structure:<br>";
    while ($row = $check_result->fetch_assoc()) {
        echo $row['Field'] . " - " . $row['Type'] . "<br>";
    }
    echo "<br>";

    // Now get the passwords
    $query = "SELECT student_id, password FROM students_tbl";
    $result = $conn->query($query);

    $updated_count = 0;

    while ($row = $result->fetch_assoc()) {
        $student_id = $row['student_id'];
        $plaintext_password = $row['password'];

        // Only rehash if the password is exactly '123'
        if ($plaintext_password === '123') {
            // Hash and update
            $hashed_password = password_hash($plaintext_password, PASSWORD_DEFAULT);

            $update_query = "UPDATE students_tbl SET password = ? WHERE student_id = ?";
            $stmt = $conn->prepare($update_query);
            $stmt->bind_param("si", $hashed_password, $student_id);
            
            if ($stmt->execute()) {
                echo "Rehashed password for student_id: $student_id<br>";
                echo "Old password: $plaintext_password<br>";
                echo "New hashed password: $hashed_password<br>";
                echo "Verification test: " . (password_verify('123', $hashed_password) ? "SUCCESS" : "FAILED") . "<br><br>";
                $updated_count++;
            } else {
                echo "Failed to update student_id: $student_id<br>";
            }
        }
    }

    echo "<br>Total updated passwords: $updated_count";
} catch (Exception $e) {
    echo "Error: " . $e->getMessage();
}

// Test a specific password
$test_password = '123';
$test_hash = password_hash($test_password, PASSWORD_DEFAULT);
echo "<br><br>Test password verification:<br>";
echo "Password: $test_password<br>";
echo "Hash: $test_hash<br>";
echo "Verify: " . (password_verify($test_password, $test_hash) ? "SUCCESS" : "FAILED") . "<br>";
?> 
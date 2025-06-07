<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST');
header('Access-Control-Allow-Headers: Content-Type');

require ('db_connection.php');

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $loginUser = $_POST['loginUser'];
    $loginPass = $_POST['loginPass'];

    // Debug: Log the received username and password length
    error_log("Login attempt for username: " . $loginUser);
    error_log("Received password length: " . strlen($loginPass));

    // Fetch user info including the hashed password
    $sql = "SELECT student_id, fk_section_id, first_name, last_name, password 
            FROM students_tbl 
            WHERE username = ?";

    $stmt = $conn->prepare($sql);
    $stmt->bind_param("s", $loginUser);
    $stmt->execute();
    $result = $stmt->get_result();

    if ($result->num_rows > 0) {
        $row = $result->fetch_assoc();
        
        // Debug: Log the stored hash (first 10 characters only for security)
        error_log("Stored hash starts with: " . substr($row['password'], 0, 10));
        
        // Verify the password using password_verify
        if (password_verify($loginPass, $row['password'])) {
            // Debug: Log successful verification
            error_log("Password verification successful");
            // Remove password field before sending back to client
            unset($row['password']);
            echo json_encode($row);
        } else {
            // Debug: Log failed verification
            error_log("Password verification failed");
            echo json_encode([
                'error' => 'Incorrect Username or Password. Try again.'
            ]);
        }
    } else {
        // Debug: Log user not found
        error_log("User not found: " . $loginUser);
        echo json_encode([
            'error' => 'Incorrect Username or Password. Try again.'
        ]);
    }
} else {
    echo json_encode([
        'error' => 'Invalid request method'
    ]);
}

$conn->close();
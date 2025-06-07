<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST');
header('Access-Control-Allow-Headers: Content-Type');

require('db_connection.php');

// Enable error reporting for debugging
error_reporting(E_ALL);
ini_set('display_errors', 1);

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $loginUser = $_POST['loginUser'];
    $loginPass = $_POST['loginPass'];

    // Debug information
    $debug = [
        'username' => $loginUser,
        'password_length' => strlen($loginPass),
        'request_method' => $_SERVER['REQUEST_METHOD'],
        'content_type' => $_SERVER['CONTENT_TYPE'] ?? 'not set'
    ];

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
        
        // Add hash information to debug data
        $debug['hash_found'] = true;
        $debug['hash_length'] = strlen($row['password']);
        $debug['hash_prefix'] = substr($row['password'], 0, 10);
        
        // Verify the password using password_verify
        if (password_verify($loginPass, $row['password'])) {
            $debug['verification'] = 'success';
            // Remove password field before sending back to client
            unset($row['password']);
            echo json_encode([
                'success' => true,
                'user_data' => $row,
                'debug' => $debug
            ]);
        } else {
            $debug['verification'] = 'failed';
            echo json_encode([
                'error' => 'Incorrect Username or Password. Try again.',
                'debug' => $debug
            ]);
        }
    } else {
        $debug['user_found'] = false;
        echo json_encode([
            'error' => 'Incorrect Username or Password. Try again.',
            'debug' => $debug
        ]);
    }
} else {
    echo json_encode([
        'error' => 'Invalid request method',
        'debug' => [
            'request_method' => $_SERVER['REQUEST_METHOD']
        ]
    ]);
}

$conn->close(); 
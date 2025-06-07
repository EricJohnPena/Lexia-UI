<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET');
header('Access-Control-Allow-Headers: Content-Type');

require('db_connection.php');

// Enable error reporting for debugging
error_reporting(E_ALL);
ini_set('display_errors', 1);

$username = $_GET['username'] ?? '';

if (empty($username)) {
    echo json_encode(['error' => 'Username is required']);
    exit;
}

$sql = "SELECT student_id, username, password, first_name, last_name 
        FROM students_tbl 
        WHERE username = ?";

try {
    $stmt = $conn->prepare($sql);
    $stmt->bind_param("s", $username);
    $stmt->execute();
    $result = $stmt->get_result();

    if ($result->num_rows > 0) {
        $row = $result->fetch_assoc();
        
        // Get hash information
        $hash = $row['password'];
        $hash_info = [
            'length' => strlen($hash),
            'prefix' => substr($hash, 0, 10),
            'is_bcrypt' => strpos($hash, '$2y$') === 0,
            'hash' => $hash // Show full hash for debugging
        ];
        
        // Remove password from response
        unset($row['password']);
        
        echo json_encode([
            'success' => true,
            'user_data' => $row,
            'hash_info' => $hash_info
        ]);
    } else {
        echo json_encode([
            'error' => 'User not found',
            'username' => $username
        ]);
    }
} catch (Exception $e) {
    echo json_encode([
        'error' => 'Database error',
        'message' => $e->getMessage()
    ]);
}

$conn->close(); 
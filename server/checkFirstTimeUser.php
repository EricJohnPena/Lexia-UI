<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST');
header('Access-Control-Allow-Headers: Content-Type');

require_once 'db_connection.php';

if ($_SERVER['REQUEST_METHOD'] === 'GET' || $_SERVER['REQUEST_METHOD'] === 'POST') {
    // Get the student_id from either GET or POST
    $student_id = isset($_GET['student_id']) ? $_GET['student_id'] : (isset($_POST['student_id']) ? $_POST['student_id'] : null);

    if (empty($student_id)) {
        echo json_encode([
            'success' => false,
            'error' => 'Student ID is required'
        ]);
        exit;
    }

    try {
        // Check if the student exists and get their first-time status
        $stmt = $conn->prepare("SELECT is_first_time FROM students_tbl WHERE student_id = ?");
        $stmt->bind_param("s", $student_id);
        $stmt->execute();
        $result = $stmt->get_result();
        
        if ($result->num_rows === 0) {
            throw new Exception("Student not found");
        }

        $row = $result->fetch_assoc();
        $is_first_time = (bool)$row['is_first_time'];

        echo json_encode([
            'success' => true,
            'is_first_time' => $is_first_time
        ]);

    } catch (Exception $e) {
        echo json_encode([
            'success' => false,
            'error' => $e->getMessage()
        ]);
    }

    $stmt->close();
} else {
    echo json_encode([
        'success' => false,
        'error' => 'Invalid request method'
    ]);
}

$conn->close(); 
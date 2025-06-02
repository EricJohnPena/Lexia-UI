<?php
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST');
header('Access-Control-Allow-Headers: Content-Type');

// Enable error reporting for debugging
error_reporting(E_ALL);
ini_set('display_errors', 1);

require_once 'db_connection.php';

// Get parameters from POST request
$student_id = isset($_POST['student_id']) ? $_POST['student_id'] : null;
$subject_id = isset($_POST['subject_id']) ? $_POST['subject_id'] : null;

// Validate parameters
if (!$student_id || !$subject_id) {
    echo json_encode(['error' => 'Missing required parameters']);
    exit;
}

// SQL to get the comments
$sql = "SELECT 
            generated_comment,
            teacher_comment
        FROM 
            student_progress_avg
        WHERE 
            student_id = ? 
            AND fk_subject_id = ?";

// Prepare statement
$stmt = $conn->prepare($sql);

// Bind parameters
$stmt->bind_param("ii", $student_id, $subject_id);

// Execute query
$stmt->execute();

// Get results
$result = $stmt->get_result();

// Fetch data
if ($row = $result->fetch_assoc()) {
    $response = array(
        'generated_comment' => $row['generated_comment'] ?? null,
        'teacher_comment' => $row['teacher_comment'] ?? null
    );
} else {
    $response = array(
        'generated_comment' => null,
        'teacher_comment' => null
    );
}

// Return JSON response
echo json_encode($response);

// Close connection
$conn->close();
?> 
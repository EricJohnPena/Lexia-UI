<?php
header("Content-Type: application/json");

include 'db_connection.php';

$student_id = isset($_GET['student_id']) ? intval($_GET['student_id']) : 0;
$lesson_id = isset($_GET['lesson_id']) ? intval($_GET['lesson_id']) : 0;

if ($student_id === 0 || $lesson_id === 0) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid parameters: student_id and lesson_id are required."]);
    exit;
}

$query = "
    SELECT completion_status 
    FROM students_progress_tbl 
    WHERE student_id = ? AND lesson_id = ?
    LIMIT 1
";

$stmt = $conn->prepare($query);
$stmt->bind_param("ii", $student_id, $lesson_id);
$stmt->execute();
$result = $stmt->get_result();

if ($row = $result->fetch_assoc()) {
    echo json_encode($row['completion_status'] === 'completed');
} else {
    echo json_encode(false);
}

$stmt->close();
$conn->close();

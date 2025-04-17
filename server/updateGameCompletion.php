<?php
header("Content-Type: application/json");

include 'db_connection.php';

$student_id = isset($_POST['student_id']) ? intval($_POST['student_id']) : 0;
$lesson_id = isset($_POST['lesson_id']) ? intval($_POST['lesson_id']) : 0;
$game_mode_id = isset($_POST['game_mode_id']) ? intval($_POST['game_mode_id']) : 0;
$subject_id = isset($_POST['subject_id']) ? intval($_POST['subject_id']) : 0;

if ($student_id === 0 || $lesson_id === 0 || $game_mode_id === 0 || $subject_id === 0) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid parameters."]);
    exit;
}

// Check if the record exists
$checkQuery = "
    SELECT progress_id 
    FROM students_progress_tbl 
    WHERE student_id = ? AND lesson_id = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
";
$stmt = $conn->prepare($checkQuery);
$stmt->bind_param("iiii", $student_id, $lesson_id, $game_mode_id, $subject_id);
$stmt->execute();
$result = $stmt->get_result();

if ($result->num_rows > 0) {
    // Record exists, update it
    $updateQuery = "
        UPDATE students_progress_tbl 
        SET no_of_attempts = no_of_attempts + 1, completion_status = 'completed' 
        WHERE student_id = ? AND lesson_id = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
    ";
    $updateStmt = $conn->prepare($updateQuery);
    $updateStmt->bind_param("iiii", $student_id, $lesson_id, $game_mode_id, $subject_id);

    if ($updateStmt->execute()) {
        echo json_encode(["success" => true, "message" => "Record updated successfully."]);
    } else {
        http_response_code(500);
        echo json_encode(["error" => $updateStmt->error]);
    }

    $updateStmt->close();
} else {
    // Record does not exist, insert it
    $insertQuery = "
        INSERT INTO students_progress_tbl (student_id, lesson_id, fk_game_mode_id, fk_subject_id, no_of_attempts, completion_status)
        VALUES (?, ?, ?, ?, 1, 'completed')
    ";
    $insertStmt = $conn->prepare($insertQuery);
    $insertStmt->bind_param("iiii", $student_id, $lesson_id, $game_mode_id, $subject_id);

    if ($insertStmt->execute()) {
        echo json_encode(["success" => true, "message" => "Record inserted successfully."]);
    } else {
        http_response_code(500);
        echo json_encode(["error" => $insertStmt->error]);
    }

    $insertStmt->close();
}

$stmt->close();
$conn->close();
?>

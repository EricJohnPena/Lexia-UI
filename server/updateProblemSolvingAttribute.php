<?php
header("Content-Type: application/json");

include 'db_connection.php';

// Retrieve and validate parameters
$student_id = isset($_POST['student_id']) ? intval($_POST['student_id']) : 0;
$lesson_id = isset($_POST['lesson_id']) ? intval($_POST['lesson_id']) : 0;
$game_mode_id = isset($_POST['game_mode_id']) ? intval($_POST['game_mode_id']) : 0;
$subject_id = isset($_POST['subject_id']) ? intval($_POST['subject_id']) : 0;
$problem_solving = isset($_POST['problem_solving']) ? intval($_POST['problem_solving']) : -1;

// Log received parameters for debugging
error_log("Received parameters: student_id=$student_id, lesson_id=$lesson_id, game_mode_id=$game_mode_id, subject_id=$subject_id, problem_solving=$problem_solving");

if ($student_id === 0 || $lesson_id === 0 || $game_mode_id === 0 || $subject_id === 0 || $problem_solving < 0) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid parameters. Ensure all required fields are provided and valid."]);
    exit;
}

// Check if the record exists
$checkQuery = "
    SELECT progress_id 
    FROM students_progress_tbl 
    WHERE student_id = ? AND lesson_id = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
";
$checkStmt = $conn->prepare($checkQuery);
if (!$checkStmt) {
    error_log("Prepare failed for checkQuery: " . $conn->error);
    http_response_code(500);
    echo json_encode(["error" => "Database error during checkQuery preparation."]);
    exit;
}
$checkStmt->bind_param("iiii", $student_id, $lesson_id, $game_mode_id, $subject_id);
$checkStmt->execute();
$checkStmt->store_result();

if ($checkStmt->num_rows > 0) {
    // Record exists, update it
    $updateQuery = "
        UPDATE students_progress_tbl 
        SET problem_solving_skills = ?
        WHERE student_id = ? AND lesson_id = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
    ";
    $updateStmt = $conn->prepare($updateQuery);
    if (!$updateStmt) {
        error_log("Prepare failed for updateQuery: " . $conn->error);
        http_response_code(500);
        echo json_encode(["error" => "Database error during updateQuery preparation."]);
        exit;
    }
    $updateStmt->bind_param("iiiii", $problem_solving, $student_id, $lesson_id, $game_mode_id, $subject_id);

    if ($updateStmt->execute()) {
        echo json_encode(["success" => true, "message" => "Problem-solving updated successfully."]);
    } else {
        error_log("Execute failed for updateQuery: " . $updateStmt->error);
        http_response_code(500);
        echo json_encode(["error" => $updateStmt->error]);
    }

    $updateStmt->close();
} else {
    // Record does not exist, insert it
    $insertQuery = "
        INSERT INTO students_progress_tbl (student_id, lesson_id, fk_game_mode_id, fk_subject_id, problem_solving_skills)
        VALUES (?, ?, ?, ?, ?)
    ";
    $insertStmt = $conn->prepare($insertQuery);
    if (!$insertStmt) {
        error_log("Prepare failed for insertQuery: " . $conn->error);
        http_response_code(500);
        echo json_encode(["error" => "Database error during insertQuery preparation."]);
        exit;
    }
    $insertStmt->bind_param("iiiii", $student_id, $lesson_id, $game_mode_id, $subject_id, $problem_solving);

    if ($insertStmt->execute()) {
        echo json_encode(["success" => true, "message" => "Problem-solving record inserted successfully."]);
    } else {
        error_log("Execute failed for insertQuery: " . $insertStmt->error);
        http_response_code(500);
        echo json_encode(["error" => $insertStmt->error]);
    }

    $insertStmt->close();
}

$checkStmt->close();
$conn->close();
?>

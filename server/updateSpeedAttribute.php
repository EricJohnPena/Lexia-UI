<?php
header("Content-Type: application/json");

include 'db_connection.php';

// Retrieve and validate parameters
$student_id = isset($_POST['student_id']) ? intval($_POST['student_id']) : 0;
$module_number = isset($_POST['module_number']) ? intval($_POST['module_number']) : 0;
$game_mode_id = isset($_POST['game_mode_id']) ? intval($_POST['game_mode_id']) : 0;
$subject_id = isset($_POST['subject_id']) ? intval($_POST['subject_id']) : 0;
$speed = isset($_POST['speed']) ? intval($_POST['speed']) : 0;

// Log received parameters for debugging
error_log("Received parameters: student_id=$student_id, module_number=$module_number, game_mode_id=$game_mode_id, subject_id=$subject_id, speed=$speed");

if ($student_id === 0 || $module_number === 0 || $game_mode_id === 0 || $subject_id === 0 || $speed === 0) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid parameters. Ensure all required fields are provided and valid."]);
    exit;
}

// Check if the record exists
$checkQuery = "
    SELECT progress_id 
    FROM students_progress_tbl 
    WHERE student_id = ? AND module_number = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
";
$checkStmt = $conn->prepare($checkQuery);
$checkStmt->bind_param("iiii", $student_id, $module_number, $game_mode_id, $subject_id);
$checkStmt->execute();
$checkStmt->store_result();

if ($checkStmt->num_rows > 0) {
    // Record exists, update it
    $updateQuery = "
        UPDATE students_progress_tbl 
        SET speed = ?
        WHERE student_id = ? AND module_number = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
    ";
    $updateStmt = $conn->prepare($updateQuery);
    $updateStmt->bind_param("iiiii", $speed, $student_id, $module_number, $game_mode_id, $subject_id);

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
        INSERT INTO students_progress_tbl (student_id, module_number, fk_game_mode_id, fk_subject_id, speed)
        VALUES (?, ?, ?, ?, ?)
    ";
    $insertStmt = $conn->prepare($insertQuery);
    $insertStmt->bind_param("iiiii", $student_id, $module_number, $game_mode_id, $subject_id, $speed);

    if ($insertStmt->execute()) {
        echo json_encode(["success" => true, "message" => "Record inserted successfully."]);
    } else {
        http_response_code(500);
        echo json_encode(["error" => $insertStmt->error]);
    }

    $insertStmt->close();
}

$checkStmt->close();
$conn->close();
?>

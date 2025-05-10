<?php
header("Content-Type: application/json");

include 'db_connection.php';

// Start transaction
$conn->begin_transaction();

try {
    // Retrieve and validate parameters
    $student_id = isset($_POST['student_id']) ? intval($_POST['student_id']) : 0;
    $module_number = isset($_POST['module_number']) ? intval($_POST['module_number']) : 0;
    $game_mode_id = isset($_POST['game_mode_id']) ? intval($_POST['game_mode_id']) : 0;
    $subject_id = isset($_POST['subject_id']) ? intval($_POST['subject_id']) : 0;
    $retention = isset($_POST['retention']) ? intval($_POST['retention']) : 0;

    // Log received parameters for debugging
    error_log("Received parameters: student_id=$student_id, module_number=$module_number, game_mode_id=$game_mode_id, subject_id=$subject_id, retention=$retention");

    if ($student_id === 0 || $module_number === 0 || $game_mode_id === 0 || $subject_id === 0 || $retention === 0) {
        throw new Exception("Invalid parameters. Ensure all required fields are provided and valid.");
    }

    // Check if the record exists
    $checkQuery = "
        SELECT progress_id 
        FROM students_progress_tbl 
        WHERE student_id = ? AND module_number = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
        FOR UPDATE
    ";
    $checkStmt = $conn->prepare($checkQuery);
    $checkStmt->bind_param("iiii", $student_id, $module_number, $game_mode_id, $subject_id);
    $checkStmt->execute();
    $checkStmt->store_result();

    if ($checkStmt->num_rows > 0) {
        // Record exists, update it
        $updateQuery = "
            UPDATE students_progress_tbl 
            SET retention = ?
            WHERE student_id = ? AND module_number = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
        ";
        $updateStmt = $conn->prepare($updateQuery);
        $updateStmt->bind_param("iiiii", $retention, $student_id, $module_number, $game_mode_id, $subject_id);

        if (!$updateStmt->execute()) {
            throw new Exception($updateStmt->error);
        }
        $updateStmt->close();
    } else {
        // Record does not exist, insert it
        $insertQuery = "
            INSERT INTO students_progress_tbl (student_id, module_number, fk_game_mode_id, fk_subject_id, retention)
            VALUES (?, ?, ?, ?, ?)
        ";
        $insertStmt = $conn->prepare($insertQuery);
        $insertStmt->bind_param("iiiii", $student_id, $module_number, $game_mode_id, $subject_id, $retention);

        if (!$insertStmt->execute()) {
            throw new Exception($insertStmt->error);
        }
        $insertStmt->close();
    }

    $checkStmt->close();
    
    // Commit transaction
    $conn->commit();
    echo json_encode(["success" => true, "message" => "Record updated successfully."]);

} catch (Exception $e) {
    // Rollback transaction on error
    $conn->rollback();
    http_response_code(500);
    echo json_encode(["error" => $e->getMessage()]);
}

$conn->close();
?>

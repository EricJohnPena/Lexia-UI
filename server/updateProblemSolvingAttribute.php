<?php
header("Content-Type: application/json");

include 'db_connection.php';

$student_id = isset($_POST['student_id']) ? intval($_POST['student_id']) : 0;
$lesson_id = isset($_POST['lesson_id']) ? intval($_POST['lesson_id']) : 0;
$game_mode_id = isset($_POST['game_mode_id']) ? intval($_POST['game_mode_id']) : 0;
$subject_id = isset($_POST['subject_id']) ? intval($_POST['subject_id']) : 0;
$problem_solving = isset($_POST['problem_solving']) ? intval($_POST['problem_solving']) : 0;

if ($student_id === 0 || $lesson_id === 0 || $game_mode_id === 0 || $subject_id === 0 || $problem_solving < 0) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid parameters."]);
    exit;
}

$checkQuery = "
    SELECT progress_id 
    FROM students_progress_tbl 
    WHERE student_id = ? AND lesson_id = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
";
$checkStmt = $conn->prepare($checkQuery);
$checkStmt->bind_param("iiii", $student_id, $lesson_id, $game_mode_id, $subject_id);
$checkStmt->execute();
$checkStmt->store_result();

if ($checkStmt->num_rows > 0) {
    $updateQuery = "
        UPDATE students_progress_tbl 
        SET problem_solving_skills = ?
        WHERE student_id = ? AND lesson_id = ? AND fk_game_mode_id = ? AND fk_subject_id = ?
    ";
    $updateStmt = $conn->prepare($updateQuery);
    $updateStmt->bind_param("iiiii", $problem_solving, $student_id, $lesson_id, $game_mode_id, $subject_id);

    if ($updateStmt->execute()) {
        echo json_encode(["success" => true, "message" => "Problem-solving updated successfully."]);
    } else {
        http_response_code(500);
        echo json_encode(["error" => $updateStmt->error]);
    }

    $updateStmt->close();
} else {
    $insertQuery = "
        INSERT INTO students_progress_tbl (student_id, lesson_id, fk_game_mode_id, fk_subject_id, problem_solving_skills)
        VALUES (?, ?, ?, ?, ?)
    ";
    $insertStmt = $conn->prepare($insertQuery);
    $insertStmt->bind_param("iiiii", $student_id, $lesson_id, $game_mode_id, $subject_id, $problem_solving);

    if ($insertStmt->execute()) {
        echo json_encode(["success" => true, "message" => "Problem-solving record inserted successfully."]);
    } else {
        http_response_code(500);
        echo json_encode(["error" => $insertStmt->error]);
    }

    $insertStmt->close();
}

$checkStmt->close();
$conn->close();
?>

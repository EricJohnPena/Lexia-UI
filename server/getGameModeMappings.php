<?php
header("Content-Type: application/json");

include 'db_connection.php';

$subject_id = isset($_GET['subject_id']) ? intval($_GET['subject_id']) : 0;
$module_id = isset($_GET['module_id']) ? intval($_GET['module_id']) : 0;
$lesson_id = isset($_GET['lesson_id']) ? intval($_GET['lesson_id']) : 0;

if ($subject_id === 0 || $module_id === 0 || $lesson_id === 0) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid parameters."]);
    exit;
}

$query = "
    SELECT gm.game_mode_name
    FROM game_mode_mapping_tbl gmm
    INNER JOIN game_modes_tbl gm ON gmm.fk_game_mode_id = gm.game_mode_id
    WHERE gmm.fk_subject_id = ? AND gmm.fk_module_id = ? AND gmm.fk_lesson_id = ?
";

$stmt = $conn->prepare($query);
$stmt->bind_param("iii", $subject_id, $module_id, $lesson_id);
$stmt->execute();
$result = $stmt->get_result();

$gameModes = [];
while ($row = $result->fetch_assoc()) {
    $gameModes[] = $row['game_mode_name'];
}

echo json_encode($gameModes);

$stmt->close();
$conn->close();

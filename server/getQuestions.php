<?php
include 'db_connection.php';

$subjectId = $_POST['fk_subject_id'];
$moduleNumber = $_POST['module_number'];
$lessonNumber = $_POST['lesson_number'];
$gameMode = $_POST['game_mode'];

$query = $conn->prepare("
    SELECT question_text, correct_answer 
    FROM questions_tbl 
    WHERE fk_subject_id = ? AND game_mode = ? AND fk_module_id = (
        SELECT module_id FROM modules_tbl WHERE module_number = ? AND fk_subject_id = ?
    ) AND fk_module_id IN (
        SELECT fk_module_id FROM lessons_tbl WHERE lesson_number = ?
    )
");
$query->bind_param("isisi", $subjectId, $gameMode, $moduleNumber, $subjectId, $lessonNumber);
$query->execute();
$result = $query->get_result();

$questions = [];
while ($row = $result->fetch_assoc()) {
    $questions[] = [
        'questionText' => $row['question_text'],
        'answer' => $row['correct_answer']
    ];
}

echo json_encode(['questions' => $questions]);
?>

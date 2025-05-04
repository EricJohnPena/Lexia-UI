<?php
header('Content-Type: application/json');
header("Access-Control-Allow-Origin: *");
require('db_connection.php');

$studentId = isset($_POST['student_id']) ? (int)$_POST['student_id'] : 0;
$sql = "
    SELECT
        CEIL(AVG(accuracy)) AS accuracy,
        CEIL(AVG(speed)) AS speed,
        CEIL(AVG(problem_solving_skills)) AS problem_solving_skills,
        CEIL(AVG(vocabulary_range)) AS vocabulary_range,
        CEIL(AVG(consistency)) AS consistency,
        CEIL(AVG(retention)) AS retention
    FROM students_progress_tbl
    WHERE student_id = $studentId
";

$result = $conn->query($sql);
$radarItems = [];

if ($result && $result->num_rows > 0) {
    $row = $result->fetch_assoc();
    $radarItems[] = array_map('intval', $row);
} else {
    $radarItems[] = ['error' => 'No data found'];
}

echo json_encode($radarItems);
$conn->close();
?>

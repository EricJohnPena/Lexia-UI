<?php
header("Content-Type: application/json");

include 'db_connection.php';

$subject_id = isset($_POST['fk_subject_id']) ? intval($_POST['fk_subject_id']) : 0;
$student_id = isset($_POST['student_id']) ? intval($_POST['student_id']) : 0;

if ($subject_id === 0 || $student_id === 0) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid parameters."]);
    exit;
}

$query = "
    SELECT 
        m.module_number, 
        m.module_number, 
        (
            SELECT COUNT(*) 
            FROM students_progress_tbl sp
            WHERE sp.module_number = m.module_number 
              AND sp.student_id = ? 
              AND sp.fk_subject_id = ?
        ) >= 3 AS is_completed
    FROM modules_tbl m
    WHERE m.fk_subject_id = ?
    ORDER BY m.module_number
";

$stmt = $conn->prepare($query);
$stmt->bind_param("iii", $student_id, $subject_id, $subject_id);
$stmt->execute();
$result = $stmt->get_result();

$modules = [];
while ($row = $result->fetch_assoc()) {
    $modules[] = [
        "module_number" => $row["module_number"],
        "is_completed" => boolval($row["is_completed"])
    ];
}

echo json_encode($modules);

$stmt->close();
$conn->close();
?>

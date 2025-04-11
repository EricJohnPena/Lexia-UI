<?php
header("Content-Type: application/json");

// Database connection
$host = "localhost";
$username = "root";
$password = "";
$database = "mydb";

$conn = new mysqli($host, $username, $password, $database);

if ($conn->connect_error) {
    die(json_encode(["error" => "Database connection failed: " . $conn->connect_error]));
}

// Get parameters
$subject_id = isset($_GET['subject_id']) ? intval($_GET['subject_id']) : 0;
$module_id = isset($_GET['module_id']) ? intval($_GET['module_id']) : 0;
$lesson_id = isset($_GET['lesson_id']) ? intval($_GET['lesson_id']) : 0;

if ($subject_id === 0 || $module_id === 0 || $lesson_id === 0) {
    echo json_encode(["error" => "Invalid parameters."]);
    exit;
}

// Fetch questions
$query = $conn->prepare("
    SELECT question_text, answer 
    FROM jumbled_letters_questions 
    WHERE fk_subject_id = ? AND fk_module_id = ? AND fk_lesson_id = ?
");
$query->bind_param("iii", $subject_id, $module_id, $lesson_id);
$query->execute();
$result = $query->get_result();

$questions = [];
while ($row = $result->fetch_assoc()) {
    $questions[] = [
        "questionText" => $row["question_text"],
        "answer" => $row["answer"]
    ];
}

echo json_encode(["questions" => $questions]);

$query->close();
$conn->close();
?>

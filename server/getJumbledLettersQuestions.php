<?php
header("Content-Type: application/json");

// Database connection
$host = "localhost";
$username = "root";
$password = "";
$database = "lexia_ui";

$conn = new mysqli($host, $username, $password, $database);

if ($conn->connect_error) {
    http_response_code(500);
    die(json_encode(["error" => "Database connection failed: " . $conn->connect_error]));
}

// Get parameters
$subject_id = isset($_GET['subject_id']) ? intval($_GET['subject_id']) : 0;
$module_id = isset($_GET['module_id']) ? intval($_GET['module_id']) : 0;
$lesson_id = isset($_GET['lesson_id']) ? intval($_GET['lesson_id']) : 0;

if ($subject_id === 0 || $module_id === 0 || $lesson_id === 0) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid parameters."]);
    exit;
}

// Debugging: Log the received parameters
error_log("Received parameters: subject_id=$subject_id, module_id=$module_id, lesson_id=$lesson_id");

// Fetch questions
$query = $conn->prepare("
    SELECT question_text, answer 
    FROM jumbled_letters_questions 
    WHERE fk_subject_id = ? AND fk_module_id = ? AND fk_lesson_id = ?
");

if (!$query) {
    http_response_code(500);
    die(json_encode(["error" => "Failed to prepare query: " . $conn->error]));
}

$query->bind_param("iii", $subject_id, $module_id, $lesson_id);

if (!$query->execute()) {
    http_response_code(500);
    die(json_encode(["error" => "Query execution failed: " . $query->error]));
}

$result = $query->get_result();

if (!$result) {
    http_response_code(500);
    die(json_encode(["error" => "Failed to fetch result: " . $query->error]));
}

$questions = [];
while ($row = $result->fetch_assoc()) {
    $questions[] = [
        "questionText" => $row["question_text"],
        "answer" => $row["answer"]
    ];
}

// Debugging: Log the fetched questions
error_log("Fetched questions: " . json_encode($questions));

echo json_encode(["questions" => $questions]);

$query->close();
$conn->close();
?>

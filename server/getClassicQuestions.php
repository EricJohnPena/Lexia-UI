<?php
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Headers: *");
header("Access-Control-Allow-Methods: *");

include 'db_connection.php';

try {
    // Validate database connection
    if (!$conn) {
        throw new Exception("Database connection failed.");
    }

    // Get parameters from the request
    $subject_id = isset($_GET['subject_id']) ? $_GET['subject_id'] : null;
    $module_id = isset($_GET['module_id']) ? $_GET['module_id'] : null;
    $lesson_id = isset($_GET['lesson_id']) ? $_GET['lesson_id'] : null;

    // Ensure all inputs are provided
    if (!$subject_id || !$module_id || !$lesson_id) {
        http_response_code(400);
        echo json_encode(["message" => "All parameters (subject_id, module_id, lesson_number) are required."]);
        exit;
    }

    // Build the query with required filters
    $query = "SELECT question_text, answer, image_path FROM classic_questions_tbl WHERE fk_subject_id = ? AND fk_module_id = ? AND fk_lesson_id = ?";
    $params = [$subject_id, $module_id, $lesson_id];
    $types = "iii";

    $stmt = $conn->prepare($query);

    // Validate query preparation
    if (!$stmt) {
        throw new Exception("Failed to prepare the query: " . $conn->error);
    }

    // Bind parameters dynamically
    if (!empty($params)) {
        $stmt->bind_param($types, ...$params);
    }

    $stmt->execute();

    // Validate query execution
    if ($stmt->error) {
        throw new Exception("Query execution failed: " . $stmt->error);
    }

    $results = [];
    $stmt->bind_result($question_text, $answer, $image_path);

    while ($stmt->fetch()) {
        $results[] = [
            'questionText' => $question_text,
            'answer' => $answer,
            'imagePath' => $image_path
        ];
    }

    // Return an empty string if no results are found
    if (empty($results)) {
        echo "";
    } else {
        echo json_encode(['questions' => $results], JSON_UNESCAPED_SLASHES);
    }
} catch (Exception $e) {
    http_response_code(500);
    echo json_encode(["message" => "Error: " . $e->getMessage()]);
} finally {
    // Ensure the connection is closed
    if ($conn) {
        $conn->close();
    }
}
?>

<?php
header("Content-Type: application/json");

include 'db_connection.php';

// Retrieve and validate parameters
$student_id = isset($_POST['student_id']) ? intval($_POST['student_id']) : 0;

// Log received parameters for debugging
error_log("Received parameters: student_id=$student_id");

if ($student_id === 0) {
    http_response_code(400);
    echo json_encode(["error" => "Invalid parameters. Ensure student_id is provided and valid."]);
    exit;
}

// Query to fetch speed and accuracy for the student across all sessions
$query = "
    SELECT speed, accuracy 
    FROM students_progress_tbl 
    WHERE student_id = ?
";
$stmt = $conn->prepare($query);
$stmt->bind_param("i", $student_id);
$stmt->execute();
$result = $stmt->get_result();

$speeds = [];
$accuracies = [];
while ($row = $result->fetch_assoc()) {
    if ($row['speed'] !== null) {
        $speeds[] = $row['speed'];
    }
    if ($row['accuracy'] !== null) {
        $accuracies[] = $row['accuracy'];
    }
}

// Calculate standard deviation for speed and accuracy
function calculateStandardDeviation($values) {
    if (count($values) === 0) {
        return 0; // Default to 0 if no values are present
    }
    $mean = array_sum($values) / count($values);
    $variance = array_reduce($values, function ($carry, $item) use ($mean) {
        return $carry + pow($item - $mean, 2);
    }, 0) / count($values);
    return sqrt($variance);
}

$speedStdDev = calculateStandardDeviation($speeds);
$accuracyStdDev = calculateStandardDeviation($accuracies);

// Calculate consistency score (lower variance = higher consistency, max 10)
$consistency = max(0, 10 - round(($speedStdDev + $accuracyStdDev) / 2));

// Update the consistency attribute for all records of the current student ID
$updateQuery = "
    UPDATE students_progress_tbl 
    SET consistency = ?
    WHERE student_id = ?
";
$updateStmt = $conn->prepare($updateQuery);
$updateStmt->bind_param("di", $consistency, $student_id);

if ($updateStmt->execute()) {
    echo json_encode(["success" => true, "message" => "Consistency updated for all records of the student."]);
} else {
    http_response_code(500);
    echo json_encode(["error" => $updateStmt->error]);
}

$updateStmt->close();
$conn->close();
?>

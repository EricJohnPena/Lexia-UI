<?php
// Database connection
require_once 'db_connection.php';

// Get subject_id from POST request
$subject_id = isset($_POST['subject_id']) ? $_POST['subject_id'] : 1;

// SQL to get the leaderboard data
// This query counts the number of completed lessons for each student and ranks them by subject
$sql = "SELECT 
            s.student_id,
            s.username,
            s.first_name,
            s.last_name,
            COUNT(CASE WHEN sp.completion_status = 'completed' THEN 1 END) as completed_lessons
        FROM 
            students_tbl s
        JOIN 
            students_progress_tbl sp ON s.student_id = sp.student_id
        JOIN 
            lessons_tbl l ON sp.lesson_id = l.lesson_id
        JOIN 
            modules_tbl m ON l.fk_module_number = m.module_number
        JOIN 
            subjects_tbl sub ON m.fk_subject_id = sub.subject_id
        WHERE 
            sub.subject_id = ?
        GROUP BY 
            s.student_id, s.username, s.first_name, s.last_name
        ORDER BY 
            completed_lessons DESC
        LIMIT 10";

// Prepare statement
$stmt = $conn->prepare($sql);

// Bind parameters
$stmt->bind_param("i", $subject_id);

// Execute query
$stmt->execute();

// Get results
$result = $stmt->get_result();

// Create array to store leaderboard entries
$leaderboard = array();

// Counter for rank
$rank = 1;

// Fetch data
while ($row = $result->fetch_assoc()) {
    // Create leaderboard entry with default values for null fields
    $entry = array(
        'student_id' => $row['student_id'] ?? 0,
        'username' => $row['username'] ?? 'Unknown',
        'first_name' => $row['first_name'] ?? 'Unknown',
        'last_name' => $row['last_name'] ?? 'Unknown',
        'completed_lessons' => intval($row['completed_lessons'] ?? 0), // Default to 0
        'rank' => $rank
    );
    
    // Add to leaderboard array
    $leaderboard[] = $entry;
    
    // Increment rank
    $rank++;
}

// If no data is found, return an empty array
if (empty($leaderboard)) {
    $leaderboard = array();
}

// Return JSON response
header('Content-Type: application/json');
echo json_encode($leaderboard);

// Close connection
$conn->close();
?>
<?php
// Database connection
require_once 'db_connection.php';

// Get subject_id from POST request
$subject_id = isset($_POST['subject_id']) ? $_POST['subject_id'] : 1;

// SQL to get the leaderboard data
// Now counts the number of completed modules per student in the given subject
$sql = "SELECT 
            s.student_id,
            s.username,
            s.first_name,
            s.last_name,
            COUNT(DISTINCT sp.module_number) AS completed_modules
        FROM 
            students_tbl s
        JOIN 
            students_progress_tbl sp ON s.student_id = sp.student_id
        JOIN 
            modules_tbl m ON sp.module_number = m.module_number
        WHERE 
            m.fk_subject_id = ?
            AND sp.completion_status = 'completed'
        GROUP BY 
            s.student_id, s.username, s.first_name, s.last_name
        ORDER BY 
            completed_modules DESC
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
    $entry = array(
        'student_id' => $row['student_id'] ?? 0,
        'username' => $row['username'] ?? 'Unknown',
        'first_name' => $row['first_name'] ?? 'Unknown',
        'last_name' => $row['last_name'] ?? 'Unknown',
        'completed_modules' => intval($row['completed_modules'] ?? 0),
        'rank' => $rank
    );
    
    $leaderboard[] = $entry;
    $rank++;
}

// Return JSON response
header('Content-Type: application/json');
echo json_encode($leaderboard);

// Close connection
$conn->close();
?>

<?php
// Database connection
require_once 'db_connection.php';

// Get subject_id from POST request
$subject_id = isset($_POST['subject_id']) ? $_POST['subject_id'] : 1;

// SQL to get the leaderboard data
// Now counts the number of completed modules per student in the given subject
$sql = "WITH subject_filtered_data AS (
            SELECT 
                s.student_id,
                s.username,
                s.first_name,
                s.last_name,
                sp.module_number,
                spa.avg_accuracy,
                spa.avg_speed,
                spa.avg_consistency,
                spa.avg_vocabulary_range,
                spa.avg_problem_solving_skills,
                spa.avg_retention
            FROM 
                students_tbl s
            JOIN 
                students_progress_tbl sp ON s.student_id = sp.student_id
            JOIN 
                modules_tbl m ON sp.module_number = m.module_number
            LEFT JOIN 
                student_progress_avg spa ON s.student_id = spa.student_id 
                AND m.fk_subject_id = spa.fk_subject_id
            WHERE 
                m.fk_subject_id = ?
                AND sp.completion_status = 'completed'
        )
        SELECT 
            student_id,
            username,
            first_name,
            last_name,
            COUNT(DISTINCT module_number) AS completed_modules,
            ROUND((
                COALESCE(AVG(avg_accuracy), 0) +
                COALESCE(AVG(avg_speed), 0) +
                COALESCE(AVG(avg_consistency), 0) +
                COALESCE(AVG(avg_vocabulary_range), 0) +
                COALESCE(AVG(avg_problem_solving_skills), 0) +
                COALESCE(AVG(avg_retention), 0)
            ) / 6, 2) AS average_score
        FROM 
            subject_filtered_data
        GROUP BY 
            student_id, username, first_name, last_name
        ORDER BY 
            completed_modules DESC,
            average_score DESC
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

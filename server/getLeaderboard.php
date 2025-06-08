<?php
// Database connection
require_once 'db_connection.php';

// Get subject_id from POST request
$subject_id = isset($_POST['subject_id']) ? intval($_POST['subject_id']) : 1;

// SQL: Filter directly on students_progress_tbl.fk_subject_id, count completed, avg scores
$sql = "
    SELECT 
        s.student_id,
        s.username,
        s.first_name,
        s.last_name,
        COUNT(*) AS completed_modules,
        ROUND((
            COALESCE(AVG(sp.accuracy), 0) +
            COALESCE(AVG(sp.speed), 0) +
            COALESCE(AVG(sp.consistency), 0) +
            COALESCE(AVG(sp.vocabulary_range), 0) +
            COALESCE(AVG(sp.problem_solving_skills), 0) +
            COALESCE(AVG(sp.retention), 0)
        ) / 6, 2) AS average_score
    FROM 
        students_tbl s
    JOIN 
        students_progress_tbl sp ON s.student_id = sp.student_id
    WHERE 
        sp.fk_subject_id = ?
        AND sp.completion_status = 'completed'
    GROUP BY 
        s.student_id, s.username, s.first_name, s.last_name
    ORDER BY 
        completed_modules DESC,
        average_score DESC
";

// Prepare statement
$stmt = $conn->prepare($sql);
$stmt->bind_param("i", $subject_id);
$stmt->execute();
$result = $stmt->get_result();

// Build leaderboard array
$leaderboard = [];
$rank = 1;

while ($row = $result->fetch_assoc()) {
    $leaderboard[] = [
        'student_id' => $row['student_id'] ?? 0,
        'username' => $row['username'] ?? 'Unknown',
        'first_name' => $row['first_name'] ?? 'Unknown',
        'last_name' => $row['last_name'] ?? 'Unknown',
        'completed_modules' => intval($row['completed_modules'] ?? 0),
        'average_score' => floatval($row['average_score'] ?? 0),
        'rank' => $rank++
    ];
}

// Return JSON response
header('Content-Type: application/json');
echo json_encode($leaderboard);

// Close connection
$conn->close();
?>

<?php
header("Access-Control-Allow-Origin: *");
header("Access-Control-Allow-Headers: *");
header("Access-Control-Allow-Methods: *");

include 'conn.php';

// Get parameters from the request
$user_id = $_POST['user_id'];
$subject_id = $_POST['subject_id'];

try {
    // Query to get leaderboard data
    // This joins with lessons table to get subject_id and users table to get usernames
    // Orders by score in descending order and adds rank
    $query = "
        SELECT 
            u.username,
            sp.score,
            @rank := @rank + 1 as rank
        FROM 
            (SELECT @rank := 0) r,
            students_progress_tbl sp
            INNER JOIN lessons_tbl l ON sp.lesson_id = l.lesson_id
            INNER JOIN users_tbl u ON sp.student_id = u.user_id
        WHERE 
            l.fk_subject_id = :subject_id
        GROUP BY 
            sp.student_id
        ORDER BY 
            MAX(sp.score) DESC
    ";

    // Prepare and execute the query
    $stmt = $conn->prepare($query);
    $stmt->bindParam(':subject_id', $subject_id);
    $stmt->execute();

    // Fetch all results
    $results = $stmt->fetchAll(PDO::FETCH_ASSOC);

    // Return the results as JSON
    echo json_encode($results);

} catch(PDOException $e) {
    // Return error message if something goes wrong
    http_response_code(500);
    echo json_encode(array("message" => "Error: " . $e->getMessage()));
}

// Close the connection
$conn = null;
?> 
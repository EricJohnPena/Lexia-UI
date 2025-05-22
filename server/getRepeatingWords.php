<?php
header('Content-Type: application/json');
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

include 'db_connection.php';

$repeatingWords = array();

$sql = "
    SELECT value, COUNT(*) as occurrences
FROM (
    SELECT UPPER(CAST(word AS CHAR CHARACTER SET utf8mb4) COLLATE utf8mb4_unicode_ci) AS value
    FROM crossword_data

    UNION ALL

    SELECT UPPER(CAST(answer AS CHAR CHARACTER SET utf8mb4) COLLATE utf8mb4_unicode_ci) AS value
    FROM classic_questions_tbl

    UNION ALL

    SELECT UPPER(CAST(answer AS CHAR CHARACTER SET utf8mb4) COLLATE utf8mb4_unicode_ci) AS value
    FROM jumbled_letters_questions
) AS all_words
GROUP BY value
HAVING COUNT(*) > 1
ORDER BY occurrences DESC, value
";

$result = $conn->query($sql);

if (!$result) {
    echo json_encode(["error" => "Query failed: " . $conn->error]);
    exit;
}

while ($row = $result->fetch_assoc()) {
    $repeatingWords[] = $row['value'];
}

echo json_encode(array_values(array_unique($repeatingWords)));
?>

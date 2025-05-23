<?php
header('Content-Type: application/json');

require('db_connection.php');

$conn = new mysqli($servername, $username, $password, $dbname);

if ($conn->connect_error) {
    die(json_encode(["error" => "Connection failed: " . $conn->connect_error]));
}

$subject_id = $_GET['subject_id'];
$module_id = $_GET['module_id'];


$sql = "SELECT word, start_row, start_col, horizontal, clue 
        FROM crossword_data 
        WHERE subject_id = ? AND module_id = ?";
$stmt = $conn->prepare($sql);
$stmt->bind_param("ii", $subject_id, $module_id,);
$stmt->execute();
$result = $stmt->get_result();

$data = [
    "fixedLayout" => [],
    "wordClues" => []
];

while ($row = $result->fetch_assoc()) {
    $data["fixedLayout"][] = [
        "word" => $row["word"],
        "startRow" => (int)$row["start_row"],
        "startCol" => (int)$row["start_col"],
        "horizontal" => (bool)$row["horizontal"]
    ];
    $data["wordClues"][] = [
        "word" => $row["word"],
        "clue" => $row["clue"]
    ];
}

echo json_encode($data);

$stmt->close();
$conn->close();
?>

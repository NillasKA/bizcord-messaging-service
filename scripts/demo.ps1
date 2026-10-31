param([string]$BaseUrl = 'http://localhost:8000')

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')
$channelId = [Guid]::NewGuid().ToString()
$authorId = [Guid]::NewGuid().ToString()
$body = @{ channelId = $channelId; authorId = $authorId; content = 'Hello Bizcord' } | ConvertTo-Json

$created = Invoke-WebRequest -Uri "$BaseUrl/api/messages" -Method Post -ContentType 'application/json' -Body $body
if ($created.StatusCode -ne 201) { throw 'POST did not return 201.' }
$message = $created.Content | ConvertFrom-Json
$messageUrl = "$BaseUrl/api/messages/$($message.messageId)"
Write-Output "Created message $($message.messageId) in channel $channelId"

$stored = Invoke-RestMethod -Uri $messageUrl
if ($stored.content -ne 'Hello Bizcord') { throw 'GET did not return the stored content.' }
$filtered = @(Invoke-RestMethod -Uri "$BaseUrl/api/messages?channelId=$channelId")
if ($filtered.Count -ne 1 -or $filtered[0].messageId -ne $message.messageId) { throw 'Channel filtering failed.' }

$edited = Invoke-RestMethod -Uri $messageUrl -Method Put -ContentType 'application/json' -Body (@{content = 'Edited message'} | ConvertTo-Json)
if ($edited.content -ne 'Edited message' -or $edited.postedAt -ne $message.postedAt) { throw 'PUT did not preserve the required fields.' }

$deleted = Invoke-WebRequest -Uri $messageUrl -Method Delete
if ($deleted.StatusCode -ne 204) { throw 'DELETE did not return 204.' }
try {
    Invoke-WebRequest -Uri $messageUrl | Out-Null
    throw 'Deleted message still exists.'
} catch {
    if ($null -eq $_.Exception.Response -or [int]$_.Exception.Response.StatusCode -ne 404) { throw }
}
Write-Output 'PASS: create, retrieve, filter, edit, delete, and missing-message status.'

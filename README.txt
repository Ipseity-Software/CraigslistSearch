To set up,

1. copy App.config.example to App.config
2. create a keywords.txt and banned.txt file
 * in keywords.txt, list any keywords you want to match on (both title and description)
 * in banned.txt, list any keywords you want to use to exclude spam (only title)
 * one word per line, lowercase only
3. fill out App.config with your email, SMTP, and paths to keyword files
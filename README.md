# sqs-daemon

Similar functionality as [Elastic Beanstalk worker environments](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/using-features-managing-env-tiers.html), implemented in C#/.NET utilizing BackgroundService features, long polling, concurrent http calls and so on. Useful when moving from Beanstalk woker environments towards an ecs solution.

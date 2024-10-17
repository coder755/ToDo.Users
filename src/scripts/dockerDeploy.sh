#!/bin/bash
set -e # exit on error
# Deploys docker image to AWS todo.users ECR
# Intended to be run from todo.users/src
# ./scripts/dockerDeploy.sh
# Docker needs to be running

export AWS_DEFAULT_PROFILE=coderPark

# variables
AWS_REGION="us-east-1"
SERVICE_DOT_NAME="todo.users"

ECR_LOCATION="442042533215.dkr.ecr.$AWS_REGION.amazonaws.com"
ECR_REPOSITORY="${ECR_LOCATION}/todo.instance"
DOCKER_TAG="${ECR_REPOSITORY}:${SERVICE_DOT_NAME}"

OUTPUT=`aws secretsmanager get-secret-value --secret-id todoDatabaseSecret --query SecretString --output text`
DB_USERNAME=$( jq -r  '.username' <<< "${OUTPUT}" )
DB_PASSWORD=$( jq -r  '.password' <<< "${OUTPUT}" )

# expected to be run from src folder
docker build --build-arg dbn="${DB_USERNAME}" --build-arg dbpw="${DB_PASSWORD}" --build-arg ASPNETCORE_ENVIRONMENT="Development" -t $SERVICE_DOT_NAME -f "${SERVICE_DOT_NAME}/Dockerfile" .

# push to aws
# login
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_LOCATION

# tag
docker tag $SERVICE_DOT_NAME $DOCKER_TAG

# push to ECR
docker push $DOCKER_TAG

# refresh fargate service
aws ecs update-service --cluster "todo-instance-cluster" --service "todo-instance-fargate-service" --force-new-deployment --no-cli-pager
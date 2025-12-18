output "vpc_id" {
  description = "VPC id"
  value       = aws_vpc.main.id
}

output "web_instance_id" {
  description = "EC2 instance id"
  value       = aws_instance.web.id
}

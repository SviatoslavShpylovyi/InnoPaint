variable "subscription_id" {
  type        = string
  description = "Azure subscription ID"
}

variable "location" {
  type        = string
  description = "Azure region; subscription policy allows only polandcentral and 4 others"

  validation {
    condition = contains([
      "polandcentral",
      "germanywestcentral",
      "francecentral",
      "italynorth",
      "spaincentral"
    ], var.location)

    error_message = "Subscription policy allows only 5 regions."
  }
}

variable "prefix" {
  type        = string
  description = "Prefix used when naming Azure resources"
}

variable "allowed_ips" {
  type        = list(string)
  description = "IP addresses allowed to access the development environment"
}

variable "mysql_admin_login" {
  type        = string
  description = "Administrator username for MySQL"
}

variable "github_repo" {
  type        = string
  description = "GitHub repository in owner/repository format"
}

variable "openai_model" {
  type        = string
  description = "OpenAI model used by the application"
  default     = "gpt-4o-mini"
}

variable "image_tag" {
  type        = string
  description = "Docker image tag to deploy"
  default     = "v1"
}

variable "mysql_admin_password" {
  type        = string
  description = "Administrator password for MySQL"
  sensitive   = true
}

variable "openai_api_key" {
  type        = string
  description = "OpenAI API key used by the application"
  sensitive   = true
}

variable "basic_auth_htpasswd" {
  type        = string
  description = "htpasswd value used to protect the development environment"
  sensitive   = true
}
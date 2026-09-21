group "default" {
  targets = ["cli", "webapp"]
}

target "cli" {
  context = "./Carnitas.CLI"
  dockerfile = "Dockerfile"
  tags = ["myapp/frontend:latest"]
}

target "webapp" {
  context = "./Carnitas.Web"
  dockerfile = "Dockerfile"
  tags = ["myapp/backend:latest"]
}
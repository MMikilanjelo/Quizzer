{{- define "mongodb.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "mongodb.fullname" }}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- .Values.clusterName | default (printf "%s-%s" .Release.Name (include "mongodb.name" .)) | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}

{{- define "mongodb.labels" }}
app.kubernetes.io/name: {{ include "mongodb.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/component: database
app.kubernetes.io/part-of: quizzer
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" }}
{{- end }}

{{- define "mongodb.usersSecretName" -}}
  {{- if .Values.secrets.users -}}
    {{- .Values.secrets.users -}}
  {{- else -}}
    {{- printf "%s-secrets" (include "mongodb.fullname" .) -}}
  {{- end -}}
{{- end }}

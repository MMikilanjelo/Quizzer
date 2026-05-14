{{- define "identity-service.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "identity-service.fullname" }}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{ printf "%s-%s" .Release.Name (include "identity-service.name" .) | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}

{{- define "identity-service.labels" }}
app.kubernetes.io/name: {{ include "identity-service.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/component: backend
app.kubernetes.io/part-of: quizzer
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" }}
{{- end }}

{{- define "identity-service.selectorLabels" }}
app.kubernetes.io/name: {{ include "identity-service.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "identity-service.configMapName" }}
{{- printf "%s-config" (include "identity-service.fullname" .) }}
{{- end }}

{{- define "identity-service.secretName" }}
{{- if .Values.appSecret.existingSecretName }}
{{- .Values.appSecret.existingSecretName -}}
{{- else }}
{{ printf "%s-secret" (include "identity-service.fullname" .) }}
{{- end }}
{{- end }}

{{- define "identity-service.postgresEnv" }}
- name: POSTGRES__HOST
  value: {{ .Values.postgres.host | quote }}
- name: POSTGRES__PORT
  value: {{ .Values.postgres.port | quote }}
- name: POSTGRES__DATABASE
  value: {{ .Values.postgres.name | quote }}
- name: POSTGRES__USERNAME
  value: {{ .Values.postgres.user | quote }}
- name: POSTGRES__PASSWORD
  valueFrom:
    secretKeyRef:
      name: {{ .Values.postgres.passwordSecretName | quote }}
      key: {{ .Values.postgres.passwordSecretKey | quote }}
{{- end }}

{{- define "identity-service.kafkaEnv" }}
- name: KAFKA__BOOTSTRAPSERVERS
  value: {{ .Values.kafka.bootstrapServers | quote }}
- name: KAFKA__USERNAME
  value: {{ .Values.kafka.userName | quote }}
- name: KAFKA__PASSWORD
  valueFrom:
    secretKeyRef:
      name: {{ .Values.kafka.passwordSecretName | quote }}
      key: {{ .Values.kafka.passwordSecretKey | quote }}
{{- end -}}

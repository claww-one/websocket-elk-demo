FROM mono:latest
WORKDIR /app
COPY . .
RUN mcs Program.cs -out:logger.exe -reference:System.Net.Http.dll,System.Runtime.Serialization.dll
CMD ["mono", "logger.exe"]

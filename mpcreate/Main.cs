/*
    mpcreate
    Copyright (C) 2008 Bob Mottram
    fuzzgun@gmail.com

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using sluggish.utilities;
using System.Text;
using MySql.Data;	
using MySql.Data.MySqlClient;
using ca.guitard.jeff.utility;

namespace mpcreate
{
	class MainClass
	{
		public static void Main(string[] args)
		{	
			//verbs vrb = new verbs();
			//vrb.LoadFromPropBank("/home/motters/develop/propbank");
			//vrb.LoadFromVerbNet("/home/motters/develop/verbnet-3.0");
			//vrb.SaveArray("/home/motters/Desktop/verb_classes.txt");
			//Console.WriteLine(verbs.GetVerbClasses("does a chair have legs?"));
			
			/*
			Console.WriteLine("105 = " + phoneme.ConvertNumber(105));
			Console.WriteLine(phoneme.ConvertText("Alan's  psychedelic breakfast"));
			byte[] formants = phoneme.FormantsNormalisedSimple("This is a test", 16);
			Console.WriteLine(phoneme.StringFromFormants(formants));
			for (int i = 0; i < formants.Length; i++)
				Console.Write(formants[i].ToString() + " ");
			Console.WriteLine("");
			
			int diff = phoneme.Difference("Cat food", "Dog loop", 16, 20);
			Console.WriteLine("diff = " + diff.ToString());
			diff = phoneme.Difference("Cake", "Bake", 16, 20);
			Console.WriteLine("diff = " + diff.ToString());
			Console.WriteLine(phoneme.ToNgramStandardised("This is a test",3));
			Console.WriteLine(Soundex.ToSoundexStandardised("This is a test"));
			*/
			
			
			Console.WriteLine("mpcreate: A utility for creating mindpixels");
			Console.WriteLine("Version 0.4");
			
            // default parameters for the database
            string server_name = "localhost";
            string database_name = "mindpixel";
            string user_name = "testuser";
            string password = "password";
            string mp_table_name = "mindpixels";
            string users_table_name = "users";
			//string soundex_table_name = "wordsoundex";
			
            string[] mp_fields = {
				"Id", "BINARY(16) NOT NULL", // that's 16 bytes, not 16 bits!
                "Hash", "INT",
                "Question", "TEXT",
                "YesVotes", "INT",
                "NoVotes", "INT",
                "Coherence", "FLOAT",
				"Ngram3", "FLOAT",
				"Soundex", "FLOAT",
				"Verbs", "FLOAT",
				"NYSIIS", "FLOAT",
				"MetaphonePrimary", "FLOAT",
				"MetaphoneSecondary", "FLOAT",
				"Connections", "FLOAT",
				"Emotion", "FLOAT"
            };

			string[] users_fields = {
                "TimeStamp", "DATETIME",
				"Username", "VARCHAR(100)",
				"Hash", "INT",
                "Question", "TEXT",
                "Answer", "TINYINT"
            };

			string[] soundex_fields = {
				"SoundexHash", "INT",
                "MindpixelHash", "INT",
            };
			
            // the character used to indicate that what follows is a parameter name
            const string switch_character = "-";
			
            // extract command line parameters
            ArrayList parameters = commandline.ParseCommandLineParameters(args, switch_character, GetValidParameters());

            // is help required ?
            string show_help = commandline.GetParameterValue("help", parameters);
            if (show_help == "true")
            {
                ShowHelp();
            }
            else
            {
				string conceptnet_rdf = commandline.GetParameterValue("cn", parameters);
				if (conceptnet_rdf != "")
				{
			        conceptnet grab = new conceptnet();
			        grab.ProcessConceptNet(conceptnet_rdf, "conceptnet.txt");					
				}
				else
				{				
					string wikipedia_directory = commandline.GetParameterValue("wp", parameters);
					if (wikipedia_directory != "")
					{
				        WikipediaFactGrabber grab = new WikipediaFactGrabber();
				        grab.ProcessWikipedia(wikipedia_directory, "wikipedia.txt");					
					}
					else
					{				
						string freebase_directory = commandline.GetParameterValue("fb", parameters);
						if (freebase_directory != "")
						{
							freebase fb = new freebase();
							fb.ProccessFreebase(freebase_directory, "freebase.txt");
						}
						else
						{
							CreateEmotionalWords();
							
							string server_name_str = commandline.GetParameterValue("server", parameters);
							if (server_name_str != "") server_name = server_name_str;
							
							string user_name_str = commandline.GetParameterValue("username", parameters);
							if (user_name_str != "") user_name = user_name_str;
			
							string password_str = commandline.GetParameterValue("password", parameters);
							if (password_str != "") password = password_str;						
							
							string database_str = commandline.GetParameterValue("db", parameters);
							if (database_str != "") database_name = database_str;
														
			                string load_filename = commandline.GetParameterValue("load", parameters);
							if (load_filename != "")
							{
								
								//List<string> word = new List<string>();
								//List<string> word_cooccurrence = new List<string>();
								
								//loadGACWordFrequencies(
					                //server_name,
					                //database_name,
					                //user_name,
					                //password,
								    //load_filename, 
								    //"Mind Hack",
								    //word,
									//word_cooccurrence);
								
								//Console.WriteLine(word.Count.ToString() + " words");
								//Console.WriteLine(word_cooccurrence.Count.ToString() + " word cooccurrences");								
								
								
								List<string> differential = new List<string>();
								List<int> differential_frequency = new List<int>();
								List<string> semantic_differential = new List<string>();
								
								loadGACSemanticDifferentials(
								    server_name,
								    database_name,
								    user_name,
								    password,
									load_filename, 
								    "Mind Hack",
									differential,
									differential_frequency,
								    semantic_differential);
								    
								
							    loadGAC(
					                load_filename, 
			                        "Mind Hack",
					                mp_fields,
					                users_fields,
								    soundex_fields,
					                server_name,
					                database_name, 
					                user_name, 
					                password, 
					                mp_table_name,
					                users_table_name);
								    //soundex_table_name);

								
								//UpdateConnections(
								    //server_name,
								    //database_name,
								    //user_name,
								    //password,
								    //mp_table_name,
									//soundex_table_name);
									
							}
							else
							{
				                string random_pixels_filename = commandline.GetParameterValue("random", parameters);
								if (random_pixels_filename != "")
								{					
									int no_of_random_pixels = 1;
								    SaveRandomMindpixels(random_pixels_filename, server_name, database_name, user_name, password, mp_table_name, no_of_random_pixels);
								}
								else
								{				
					                string user_pixels_filename = commandline.GetParameterValue("userpixels", parameters);
									if (user_pixels_filename != "")
									{
									    SaveUserPixels(user_pixels_filename, server_name, database_name, user_name, password, users_table_name);
									}
									else
									{				
						                string save_filename = commandline.GetParameterValue("save", parameters);
										if (save_filename != "")
										{
											SaveMindpixels(save_filename, server_name, database_name, user_name, password, mp_table_name);
										}
										else
										{
							                string question = commandline.GetParameterValue("q", parameters);
											if (question != "")
											{
							                    string answer = commandline.GetParameterValue("a", parameters);
												if (answer != "")
												{
													bool answer_value = false;
													answer = answer.ToLower();
													if ((answer == "yes") || 
													    (answer == "y") ||
													    (answer == "true") ||
													    (answer == "t"))
														answer_value = true;
																			
													int question_hash = GetHashCode(question);
																														
										            // insert the field names into a list so that we can easily search it
										            List<string> mp_fields_to_be_inserted = new List<string>();
										            List<string> mp_field_type = new List<string>();
										            for (int i = 0; i < mp_fields.Length; i += 2)
										            {
										                mp_fields_to_be_inserted.Add(mp_fields[i]);
										                mp_field_type.Add(mp_fields[i + 1]);
										            }
							
										            List<string> users_fields_to_be_inserted = new List<string>();
										            List<string> users_field_type = new List<string>();
										            for (int i = 0; i < users_fields.Length; i += 2)
										            {
										                users_fields_to_be_inserted.Add(users_fields[i]);
										                users_field_type.Add(users_fields[i + 1]);
										            }

													List<string> soundex_fields_to_be_inserted = new List<string>();
										            List<string> soundex_field_type = new List<string>();
										            for (int i = 0; i < soundex_fields.Length; i += 2)
										            {
										                soundex_fields_to_be_inserted.Add(soundex_fields[i]);
										                soundex_field_type.Add(soundex_fields[i + 1]);
										            }
													
										            // create tables if necessary
										            CreateTable(
										                server_name,
										                database_name,
										                user_name,
										                password,
										                mp_table_name,
										                mp_fields_to_be_inserted,
										                mp_field_type, 0,1,5);
																			
										            CreateTable(
										                server_name,
										                database_name,
										                user_name,
										                password,
										                users_table_name,
										                users_fields_to_be_inserted,
										                users_field_type,-1,0,1);

													
													//CreateTable(
										                //server_name,
										                //database_name,
										                //user_name,
										                //password,
										                //soundex_table_name,
										                //soundex_fields_to_be_inserted,
										                //soundex_field_type,-1,0,1);

											        //InsertWordsIntoMySql(
													    //question_hash,
											            //question,
											            //server_name,
											            //database_name,
											            //user_name,
											            //password,
											            //soundex_table_name,
											            //soundex_fields_to_be_inserted,
											            //soundex_field_type);
													
									                InsertMindpixelIntoMySql(
													    question_hash,
									                    question,
									                    answer_value,
									                    server_name,
									                    database_name,
									                    user_name,
									                    password,
									                    mp_table_name,
									                    mp_fields_to_be_inserted,
									                    mp_field_type);
																			
									                InsertMindpixelUserDataIntoMySql(
													    question_hash,
									                    question,
									                    answer_value,
									                    server_name,
									                    database_name,
									                    user_name,
									                    password,
									                    users_table_name,
									                    users_fields_to_be_inserted,
									                    users_field_type);
													
													Console.WriteLine("Mindpixel added");
												}
												else
												{
							  					    Console.WriteLine("Please specify a yes/no answer using the -a option");
												}
											}
											else
											{
												//Console.WriteLine("Please specify a question using the -q option");
											}
										}
									}
								}
							}
							
							int plot_type = 0;
							string plot_type_str = commandline.GetParameterValue("plot", parameters);
							if (plot_type_str != "")
							{
								plot_type = Convert.ToInt32(plot_type_str);
							}
							
							string map_filename = commandline.GetParameterValue("map", parameters);
							if (map_filename != "")
							{
								float[] coherence = null;
								int[] hash1 = null;
								int[] hash2 = null;
								int[] hash3 = null;
								float[] index1 = null;
								float[] index2 = null;
								float[] index3 = null;
								Console.WriteLine("Saving map...");
								ShowPlot(
					                server_name,
					                database_name, 
					                user_name, 
					                password,
								    1000,
								    map_filename,
								    false,
								    plot_type,
								    ref coherence,
								    ref hash1,
								    ref hash2,
								    ref hash3,
								    ref index1,
								    ref index2,
								    ref index3);
								Console.WriteLine("Saved " + map_filename);
							}
							
							string map_mono_filename = commandline.GetParameterValue("mapmono", parameters);
							if (map_mono_filename != "")
							{
								float[] coherence = null;
								int[] hash1 = null;
								int[] hash2 = null;
								int[] hash3 = null;
								float[] index1 = null;
								float[] index2 = null;
								float[] index3 = null;
								Console.WriteLine("Saving map...");
								ShowPlot(
					                server_name,
					                database_name, 
					                user_name, 
					                password,
								    1000,
								    map_mono_filename,
								    false,
								    plot_type,
								    ref coherence,
								    ref hash1,
								    ref hash2,
								    ref hash3,
								    ref index1,
								    ref index2,
								    ref index3);
								Console.WriteLine("Saved " + map_filename);
							}
							
							string lookup_tables_filename = commandline.GetParameterValue("lookup", parameters);
							if (lookup_tables_filename != "")
							{
								float[] coherence = null;
								int[] hash1 = null;
								int[] hash2 = null;
								int[] hash3 = null;
								float[] index1 = null;
								float[] index2 = null;
								float[] index3 = null;
								Console.Write("Generating map...");
								ShowPlot(
					                server_name,
					                database_name, 
					                user_name, 
					                password,
								    1000,
								    "",
								    true,
								    plot_type,
								    ref coherence,
								    ref hash1,
								    ref hash2,
								    ref hash3,
								    ref index1,
								    ref index2,
								    ref index3);
								Console.WriteLine("Done");
								
								SaveLookupTables(
								    lookup_tables_filename,
								    index1, index2, index3,
								    coherence);
								Console.WriteLine("Saved lookup tables to " + lookup_tables_filename);
							}
							
							
						}
					}
				}			
			}
		}
		
		#region "saving lookup tables"
		
		private static void SaveLookupTables(
            string filename,
			float[] index1, 
		    float[] index2,
		    float[] index3,
			float[] coherence)
		{			
			StreamWriter oWrite = null;
			bool file_open = false;
			try
			{
			    oWrite = File.CreateText(filename);
				file_open = true;
			}
			catch
			{
			}
			if (file_open)
			{
				oWrite.WriteLine(index1.Length.ToString());
				for (int i = 0; i < index1.Length; i++)
				{
					oWrite.WriteLine(index1[i].ToString());
					oWrite.WriteLine(index2[i].ToString());
					oWrite.WriteLine(index3[i].ToString());
				}
				oWrite.Close();
			}			
		}		
		
		#endregion
		
		#region "generate image"
		
		//static int img_width = 1000;
		static int plot_ctr;
		
		#endregion
		
		/// <summary>
		/// Generates a hash code indicative of the text string
		/// </summary>
		/// <param name="s">string</param>
		/// <returns>hash code</returns>
		private static int GetHashCode(string s)
		{
			s = s.Trim();
			s = s.ToLower();
			
			char[] ch = s.ToCharArray();
			
			int hash = 0;
            for (int i = 0; i < ch.Length; i++) {
				if (((ch[i] >= 'a') &&
				     (ch[i] <= 'z')) ||
				    ((ch[i] >= '0') &&
				     (ch[i] <= '9')))
				{
                    hash = 31*hash + ch[i];
				}
            }

			return(hash);
		}
				
		#region "mysql database"
		
        private static void CreateTable(
            string server_name,
            string database_name,
            string user_name,
            string password,
            string table_name,
            List<string> fields_to_be_inserted,
            List<string> field_type,
		    int primary_key_field,
		    int index_field1,
		    int index_field2)
        {
            MySqlConnection connection = new MySqlConnection();

            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";

            connection.ConnectionString =
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
			//Console.WriteLine("connection string: " + connection.ConnectionString);

            bool connected = false;
            string exception_str = "";
            try
            {
                connection.Open();
                connected = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection error: " + ex.Message);
            }

            if (connected)
            {
                string Query = "CREATE TABLE IF NOT EXISTS " + table_name + "(";

                for (int i = 0; i < fields_to_be_inserted.Count; i++)
                {
                    Query += fields_to_be_inserted[i] + " " + field_type[i] + ",";					
                }
			    if (primary_key_field> -1) Query += "PRIMARY KEY (" + fields_to_be_inserted[primary_key_field] + ")";
				if (index_field1 > -1)
				{
					if (primary_key_field> -1) Query += ",";
					Query += "INDEX (" + fields_to_be_inserted[index_field1] + ")";
					if (index_field2 > -1) Query += ",INDEX (" + fields_to_be_inserted[index_field2] + ")";
				}
				if (table_name == "mindpixels")
				{
					Query += ",INDEX (Ngram3),INDEX (Soundex),INDEX (Verbs),INDEX (Connections),INDEX (NYSIIS),INDEX (MetaphonePrimary),INDEX (MetaphoneSecondary),INDEX (Emotion)";
				}
				Query += ")";
				
                MySqlCommand addxml = new MySqlCommand(Query, connection);

                //Console.WriteLine("Running query:");
                //Console.WriteLine(Query);

                try
                {
                    addxml.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
					Console.WriteLine("Can't create table " + ex.Message);
                }

                connection.Close();
            }
            else
            {
                Console.WriteLine("CreateTable: Couldn't connect to database " + database_name);
                Console.WriteLine(exception_str);
            }
        }
		
		private static ArrayList RunMySqlCommand(
		    string commandText, 
		    string connectionString,
		    int no_of_fields)
	    {
			ArrayList rows = new ArrayList();
	        using (MySqlConnection connection = new MySqlConnection(connectionString))
	        {
	            try
	            {
					//Console.WriteLine("commandtext: " + commandText);
	                MySqlCommand command = new MySqlCommand(commandText, connection);
	
	                connection.Open();
	                IAsyncResult result = command.BeginExecuteReader();
	
	                int count = 0;
	                while (!result.IsCompleted)
	                {
	                    count += 1;
	                    //Console.WriteLine("Waiting ({0})", count);
	                    System.Threading.Thread.Sleep(100);
	                }
	
	                MySqlDataReader query_result = command.EndExecuteReader(result);
					while (query_result.Read())
					{
						ArrayList row = new ArrayList();
						for (int i = 0; i < no_of_fields; i++)
						{
							row.Add(query_result.GetValue(i));
						}
						rows.Add(row);
					}
					
					connection.Close();
	            }
	            catch (MySqlException ex)
	            {
	                Console.WriteLine("Error ({0}): {1}", ex.Number, ex.Message);
					connection.Close();
	            }
	            catch (InvalidOperationException ex)
	            {
	                Console.WriteLine("Error: {0}", ex.Message);
					connection.Close();
	            }
	            catch (Exception ex)
	            {
	                Console.WriteLine("Error: {0}", ex.Message);
					connection.Close();
	            }
	        }
			return(rows);
	    }
		
        static int[] emotional_word_rating;
        static string[] emotional_words;
		static int no_of_emotional_words;
		
		private static void addEmotionalWord(string word, int rating)
		{
			emotional_word_rating[no_of_emotional_words] = rating;
			emotional_words[no_of_emotional_words] = word;
			no_of_emotional_words++;
		}
		
		private static void CreateEmotionalWords()
		{	
			emotional_word_rating = new int[300];
			emotional_words = new string[300];
			no_of_emotional_words = 0;
			
            //words associated with positive emotions
            addEmotionalWord("love", 20);
            addEmotionalWord("loved", 20);
            addEmotionalWord("like", 2);
            addEmotionalWord("enjoy", 5);
            addEmotionalWord("pleasurable", 5);
            addEmotionalWord("pleasure", 5);
            addEmotionalWord("easy", 5);
            addEmotionalWord("enjoys", 5);
            addEmotionalWord("amused", 5);
            addEmotionalWord("amusing", 5);
            addEmotionalWord("entertained", 5);
            addEmotionalWord("entertaining", 5);
            addEmotionalWord("enjoyed", 5);
            addEmotionalWord("treasure", 5);
            addEmotionalWord("treasured", 5);
            addEmotionalWord("pleasant", 5);
            addEmotionalWord("joy", 5);
            addEmotionalWord("happy", 5);
            addEmotionalWord("good", 5);
            addEmotionalWord("good at", 5);
            addEmotionalWord("happiness", 5);
            addEmotionalWord("popular", 5);
            addEmotionalWord("esteem", 5);
            addEmotionalWord("worthy", 5);
            addEmotionalWord("worthwhile", 5);
            addEmotionalWord("loving", 5);
            addEmotionalWord("positive", 5);
            addEmotionalWord("protect", 5);
            addEmotionalWord("enjoyable", 10);
            addEmotionalWord("exciting", 10);
            addEmotionalWord("pleasing", 10);
            addEmotionalWord("pleased", 10);
            addEmotionalWord("encouraging", 5);
            addEmotionalWord("encouraged", 5);
            addEmotionalWord("encourage", 5);
            addEmotionalWord("invest", 4);
            addEmotionalWord("investment", 4);
            addEmotionalWord("growth", 4);
            addEmotionalWord("increase", 10);
            addEmotionalWord("increased", 10);
            addEmotionalWord("joyous", 10);
            addEmotionalWord("tidy", 2);
            addEmotionalWord("neat", 2);
            addEmotionalWord("confident", 10);
            addEmotionalWord("confidence", 10);
            addEmotionalWord("peace", 10);
            addEmotionalWord("peacefull", 10);
            addEmotionalWord("calm", 10);
            addEmotionalWord("bright", 10);
            addEmotionalWord("rosy", 10);
            addEmotionalWord("kindness", 10);
            addEmotionalWord("goodness", 10);
            addEmotionalWord("better", 2);
            addEmotionalWord("welcome", 2);
            addEmotionalWord("amusing", 2);
            addEmotionalWord("lucky", 5);
            addEmotionalWord("correct", 5);
            addEmotionalWord("hired", 5);
            addEmotionalWord("replenish", 5);
            addEmotionalWord("respect", 5);
            addEmotionalWord("respects", 5);
            addEmotionalWord("respected", 5);
            addEmotionalWord("working", 5);
            addEmotionalWord("fascinate", 5);
            addEmotionalWord("fascinated", 5);
            addEmotionalWord("fascinating", 5);
            addEmotionalWord("poetic", 5);
            addEmotionalWord("interested", 5);
            addEmotionalWord("interesting", 5);
            addEmotionalWord("divine", 10);
            addEmotionalWord("divinely", 10);
            addEmotionalWord("fantastic", 10);
            addEmotionalWord("fabulous", 10);
            addEmotionalWord("funny", 10);
            addEmotionalWord("initiative", 2);
            addEmotionalWord("genuis", 5);
            addEmotionalWord("freedom", 5);
            addEmotionalWord("devoted", 10);
            addEmotionalWord("refreshing", 10);
            addEmotionalWord("refreshed", 10);
            addEmotionalWord("enthusiast", 10);
            addEmotionalWord("enthusiastic", 10);
            addEmotionalWord("enthusiastically", 10);
            addEmotionalWord("enthused", 10);
            addEmotionalWord("enthusiasm", 10);
            

            //words associated with negative emotions
            addEmotionalWord("hate", -10);
            addEmotionalWord("hated", -10);
            addEmotionalWord("never", -2);
            addEmotionalWord("hates", -10);
            addEmotionalWord("harm", -10);
            addEmotionalWord("kill", -10);
            addEmotionalWord("kills", -10);
            addEmotionalWord("killed", -10);
            addEmotionalWord("killing", -10);
            addEmotionalWord("death", -10);
            addEmotionalWord("grave", -10);
            addEmotionalWord("victim", -10);
            addEmotionalWord("stole", -5);
            addEmotionalWord("steal", -5);
            addEmotionalWord("stolen", -5);
            addEmotionalWord("surrender", -1);
            addEmotionalWord("killing", -10);
            addEmotionalWord("offensive", -10);
            addEmotionalWord("disslike", -5);
            addEmotionalWord("frustrate", -5);
            addEmotionalWord("frustration", -5);
            addEmotionalWord("exclusion", -5);
            addEmotionalWord("empty", -5);
            addEmotionalWord("sad", -5);
            addEmotionalWord("robbed", -8);
            addEmotionalWord("crooked", -5);
            addEmotionalWord("scam", -5);
            addEmotionalWord("slave", -5);
            addEmotionalWord("evil", -10);
            addEmotionalWord("hysterical", -10);
            addEmotionalWord("hysteria", -10);
            addEmotionalWord("imposed", -5);
            addEmotionalWord("violent", -5);
            addEmotionalWord("violence", -5);
            addEmotionalWord("shot", -10);
            addEmotionalWord("genocide", -10);
            addEmotionalWord("fighting", -5);
            addEmotionalWord("paranoia", -5);
            addEmotionalWord("paranoid", -5);
            addEmotionalWord("propaganda", -5);
            addEmotionalWord("fraud", -5);
            addEmotionalWord("liar", -10);
            addEmotionalWord("lies", -10);
            addEmotionalWord("misslead", -1);
            addEmotionalWord("lied", -10);
            addEmotionalWord("hoax", -1);
            addEmotionalWord("scary", -5);
            addEmotionalWord("scared", -5);
            addEmotionalWord("scare", -5);
            addEmotionalWord("frightening", -10);
            addEmotionalWord("frightened", -10);
            addEmotionalWord("criticism", -5);
            addEmotionalWord("criticise", -5);
            addEmotionalWord("horror", -8);
            addEmotionalWord("disturbing", -8);
            addEmotionalWord("horrible", -8);
            addEmotionalWord("criticised", -5);
            addEmotionalWord("criticising", -5);
            addEmotionalWord("spam", -10);
            addEmotionalWord("damn", -10);
            addEmotionalWord("darn", -10);
            addEmotionalWord("fired", -5);
            addEmotionalWord("sacked", -5);
            addEmotionalWord("redundant", -5);
            addEmotionalWord("bad", -10);
            addEmotionalWord("too bad", -10);
            addEmotionalWord("scream", -10);
            addEmotionalWord("screamed", -10);
            addEmotionalWord("screaming", -10);
            addEmotionalWord("worse", -2);
            addEmotionalWord("stumble", -2);
            addEmotionalWord("stumbling", -2);
            addEmotionalWord("frantic", -2);
            addEmotionalWord("arrested", -10);
            addEmotionalWord("worse than", -2);
            addEmotionalWord("virus", -10);
            addEmotionalWord("bomb", -10);
            addEmotionalWord("boring", -10);
            addEmotionalWord("dumb", -2);
            addEmotionalWord("unnecessary", -2);
            addEmotionalWord("unsophisticated", -2);
            addEmotionalWord("fuck", -10);
            addEmotionalWord("fucked", -10);
            addEmotionalWord("shoot", -10);
            addEmotionalWord("incorrect", -10);
            addEmotionalWord("shot", -10);
            addEmotionalWord("shooter", -10);
            addEmotionalWord("messy", -2);
            addEmotionalWord("untidy", -2);
            addEmotionalWord("shooting", -10);
            addEmotionalWord("fucker", -10);
            addEmotionalWord("fucking", -10);
            addEmotionalWord("shit", -10);
            addEmotionalWord("bitch", -10);
            addEmotionalWord("dont like", -10);
            addEmotionalWord("bitchy", -10);
            addEmotionalWord("bitching", -10);
            addEmotionalWord("gun", -10);
            addEmotionalWord("guns", -10);
            addEmotionalWord("weapon", -10);
            addEmotionalWord("weapons", -10);
            addEmotionalWord("war", -10);
            addEmotionalWord("rifle", -10);
            addEmotionalWord("lonely", -10);
            addEmotionalWord("loser", -10);
            addEmotionalWord("invade", -10);
            addEmotionalWord("invasion", -10);
            addEmotionalWord("disease", -10);
            addEmotionalWord("sick", -10);
            addEmotionalWord("ill", -10);
            addEmotionalWord("illness", -10);
            addEmotionalWord("sickness", -10);
            addEmotionalWord("jealous", -5);
            addEmotionalWord("fear", -5);
            addEmotionalWord("feared", -5);
            addEmotionalWord("fearfull", -5);
            addEmotionalWord("nowhere", -5);
            addEmotionalWord("unpopular", -5);
            addEmotionalWord("angry", -5);
            addEmotionalWord("deluded", -5);
            addEmotionalWord("delusion", -5);
            addEmotionalWord("deserted", -5);
            addEmotionalWord("worthless", -5);
            addEmotionalWord("difficult", -2);
            addEmotionalWord("hard", -2);
            addEmotionalWord("trouble", -2);
            addEmotionalWord("troublesome", -4);
            addEmotionalWord("bicker", -5);
            addEmotionalWord("argue", -5);
            addEmotionalWord("decline", -5);
            addEmotionalWord("reduce", -5);
            addEmotionalWord("cut", -5);
            addEmotionalWord("unlucky", -5);
            addEmotionalWord("wrong", -5);
            addEmotionalWord("go wrong", -5);
            addEmotionalWord("gone wrong", -5);
            addEmotionalWord("going wrong", -5);
            addEmotionalWord("not working", -5);
		}
		
		private static float GetEmotionRating(string text)
		{
			float rating = 0;
			
			text = text.ToLower();
			for (int i = 0; i < no_of_emotional_words; i++)
			{
				if (text.Contains(emotional_words[i]))
					rating += emotional_word_rating[i];
			}
			
			rating /= 40.0f;
			
			return(rating);
		}

		private static float GetNgramIndex(string idx, int max_length)
		{
			double index = 0;
			idx = idx.ToLower();
			if (idx.Length > max_length)
				idx = idx.Substring(0, max_length);
			while (idx.Length < max_length)
				idx += "0";
			
			char[] ch = idx.ToCharArray();
			
			double mult = 1.0/3600000000.0;
			double max = 0;
            for (int i = 0; i < ch.Length; i++) {
				if (((ch[i] >= 'a') &&
				     (ch[i] <= 'z')) ||
				    ((ch[i] >= '0') &&
				     (ch[i] <= '9')))
				{
					if (ch[i] <= '9')
						index = 2*index + ((ch[i] - (int)'0' + 1)*mult);
					else
                        index = 2*index + ((ch[i] - (int)'a' + 11)*mult);
					
					max = 2*max + (36*mult);
				}
            }
			index /= max;
			//Console.WriteLine("index = " + index.ToString());
			
			return((float)index);
		}
		
        public static void InsertMindpixelIntoMySql(
		    int hash,
            string question,
            bool answer,
            string server_name,
            string database_name,
            string user_name,
            string password,
            string table_name,
            List<string> fields_to_be_inserted,
            List<string> field_type)
        {
			int plot_type = 0;
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";

			float coherence = 0.0f;
			if (answer == true) coherence = 1.0f;
			
			string index_ngram3 = phoneme.ToNgramStandardised(question, 3, false);
			string index_soundex = Soundex.ToSoundexStandardised(question, false, false);
			string index_verbs = verbs.GetVerbClasses(question);
			string index_metaphone_primary="", index_metaphone_secondary="";
			Metaphone.ToMetaphoneStandardised(question, false, ref index_metaphone_primary, ref index_metaphone_secondary);
			string index_nysiis = NYSIIS.ToNYSIISStandardised(question, false);

			float coordinate_ngram3 = GetNgramIndex(index_ngram3, 80);
			float coordinate_soundex = GetNgramIndex(index_soundex, 80);
			float coordinate_verbs = GetNgramIndex(index_verbs, 80);
			float coordinate_nysiis = GetNgramIndex(index_nysiis, 80);
			float coordinate_metaphone_primary = GetNgramIndex(index_metaphone_primary, 80);
			float coordinate_metaphone_secondary = GetNgramIndex(index_metaphone_secondary, 80);
			float coordinate_emotion = GetEmotionRating(question);
			
			plot_ctr++;
			if (plot_ctr > 10000)
			{
				float[] coherence2 = null;
				int[] hash1 = null;
				int[] hash2 = null;
				int[] hash3 = null;
				float[] index1 = null;
				float[] index2 = null;
				float[] index3 = null;
                ShowPlot(
                    server_name,
                    database_name,
                    user_name,
                    password,
		            1000,
		            "mindpixels.bmp",
				    false,
				    plot_type,
				    ref coherence2,
				    ref hash1,
				    ref hash2,
				    ref hash3,
				    ref index1,
				    ref index2,
				    ref index3);
					
				plot_ctr = 0;
			}
						
			string connection_str = 
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
			string Query = "SELECT * FROM " + table_name + " WHERE Hash = " + hash.ToString() + ";";
			//Console.WriteLine(Query);
			
			int YesVotes = 0;
			int NoVotes = 0;
			Guid best_result = Guid.Empty;
			ArrayList pixels = RunMySqlCommand(Query, connection_str, 5);
			if (pixels.Count > 0)
			{
				char[] template = Soundex.ToSoundexCode(question).ToCharArray();
				int max_score = -1;
				// update existing pixel
                for (int ctr = 0; ctr < pixels.Count; ctr++)
				{
					ArrayList row = (ArrayList)pixels[ctr];
					string result_question = (string)row[2];
  				    char[] s = Soundex.ToSoundexCode(result_question).ToCharArray();
					int score = 0;
					for (int i = 0; i < s.Length; i++)
					{						
						score += Math.Abs((int)s[i] - (int)template[i]);
					}
					if (score > max_score)
					{
						best_result = (Guid)row[0];
						max_score = score;
						YesVotes = Convert.ToInt32(row[3]);
						NoVotes = Convert.ToInt32(row[4]);
					}
					
					//Guid result_Id = (Guid)pixels.GetValue(0);
					//int result_hash = pixels.GetInt32(1);
					//int result_yes = pixels.GetInt32(3);
					//int result_no = pixels.GetInt32(4);
                }
				
				if (best_result != Guid.Empty)
				{
					// update the mindpixel coherence
					if (answer == true)
						YesVotes++;
					else
						NoVotes++;
					coherence = YesVotes / (float)(YesVotes + NoVotes);
					
				    Query = "UPDATE " + table_name + 
						" SET YesVotes='" + YesVotes.ToString() + "'," +
						" NoVotes='" + NoVotes.ToString() + "'," +
						" Coherence='" + coherence.ToString() + "'," +
					    " Ngram3='" + coordinate_ngram3.ToString() + "'," +
					    " Soundex='" + coordinate_soundex.ToString() + "'," +
						" Verbs='" + coordinate_verbs.ToString() + "'," +
					    " NYSIIS='" + coordinate_nysiis.ToString() + "'," +
					    " MetaphonePrimary='" + coordinate_metaphone_primary.ToString() + "'," +
					    " MetaphoneSecondary='" + coordinate_metaphone_secondary.ToString() + "'," +
					    " Emotion='" + coordinate_emotion.ToString() + "'" +
					    " WHERE Id=CAST(?get_id as BINARY(16));";
					
                    MySqlConnection connection = new MySqlConnection();

                    connection.ConnectionString = connection_str;

                    bool connected = false;
                    try
                    {
                        connection.Open();
                        connected = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
						connection.Close();
                    }

                    if (connected)
                    {
		                MySqlCommand update = new MySqlCommand(Query, connection);
						update.Parameters.Add("get_id", MySqlDbType.Binary).Value = best_result.ToByteArray();
		
		                //Console.WriteLine("Running update query:");
		                //Console.WriteLine(Query);
		
		                try
		                {
		                    update.ExecuteNonQuery();
		                }
		                catch (Exception excp)
		                {
		                    Exception myExcp = new Exception("Could not update mindpixel. Error: " + excp.Message, excp);
		                    throw (myExcp);
		                }
		
		                connection.Close();					
				    }	
				}
			}
			
			if (best_result == Guid.Empty)
			{
	            List<string> field_name = new List<string>();
	            List<string> field_value = new List<string>();
				
				if (answer == true)
				{
					YesVotes = 1;
					coherence = 1;
				}
				else
				{
					NoVotes = 1;
					coherence = 0;
				}
	
				field_name.Add("Id");
				field_value.Add(System.Guid.NewGuid().ToString());
				field_name.Add("Hash");
				field_value.Add(hash.ToString());
				field_name.Add("Question");
				field_value.Add(question);
				field_name.Add("YesVotes");
				field_value.Add(YesVotes.ToString());				
				field_name.Add("NoVotes");
				field_value.Add(NoVotes.ToString());				
				field_name.Add("Coherence");
				field_value.Add(coherence.ToString());
			    field_name.Add("Ngram3");
			    field_value.Add(coordinate_ngram3.ToString());
			    field_name.Add("Soundex");
			    field_value.Add(coordinate_soundex.ToString());
			    field_name.Add("Verbs");
			    field_value.Add(coordinate_verbs.ToString());
			    field_name.Add("Connections");
			    field_value.Add("0");
			    field_name.Add("NYSIIS");
			    field_value.Add(coordinate_nysiis.ToString());
			    field_name.Add("MetaphonePrimary");
			    field_value.Add(coordinate_metaphone_primary.ToString());
			    field_name.Add("MetaphoneSecondary");
			    field_value.Add(coordinate_metaphone_secondary.ToString());
			    field_name.Add("Emotion");
			    field_value.Add(coordinate_emotion.ToString());
				
				// add new pixel
	            MySqlConnection connection = new MySqlConnection();
				
				connection.ConnectionString = connection_str;
		
	            bool connected = false;
	            string exception_str = "";
	            try
	            {
	                connection.Open();
	                connected = true;
	            }
	            catch (Exception ex)
	            {
	                exception_str = ex.Message;
					connection.Close();
	            }
	
	            if (connected)
	            {
	                Query = "INSERT INTO " + table_name + "(";
	
	                for (int i = 0; i < field_name.Count; i++)
	                {
	                    if ((fields_to_be_inserted.Count == 0) ||
	                        (fields_to_be_inserted.Contains(field_name[i])))
	                    {
                            Query += field_name[i];
	                        if (i < field_name.Count - 1) Query += ",";
	                    }
	                }
	                Query += ") values(";
	                for (int i = 0; i < field_value.Count; i++)
	                {
	                    int idx = fields_to_be_inserted.IndexOf(field_name[i]);
	
	                    if ((fields_to_be_inserted.Count == 0) ||
	                        (fields_to_be_inserted.Contains(field_name[i])))
	                    {
	                        if (fields_to_be_inserted.Count > 0)
	                        {
	                            if (idx > -1)
	                            {
									if (field_type[idx].Contains("BINARY"))
									{
										field_value[i] = "?Id";
									}
									
	                                if (field_type[idx] == "DATETIME")
	                                {
	                                    DateTime d = DateTime.Parse(field_value[i]);
	                                    string d_str = d.Year.ToString() + "-" + d.Month.ToString() + "-" + d.Day.ToString() + " " + d.Hour.ToString() + ":" + d.Minute.ToString() + ":" + d.Second.ToString();
	                                    field_value[i] = d_str;
	                                }
	                            }
	                        }
	
							if (!field_type[idx].Contains("BINARY"))
	                            Query += "'" + field_value[i] + "'";
							else
								Query += field_value[i];
	                        if (i < field_value.Count - 1) Query += ",";
	                    }
	                }
	                Query += ")";
	
	                MySqlCommand addxml = new MySqlCommand(Query, connection);
	
	                //Console.WriteLine("Running insert query:");
	                //Console.WriteLine(Query);
	
	                try
	                {
						addxml.Parameters.Add("Id", MySqlDbType.Binary).Value = System.Guid.NewGuid().ToByteArray();
	                    addxml.ExecuteNonQuery();
	                }
	                catch (Exception excp)
	                {
	                    Exception myExcp = new Exception("Could not add new mindpixel. Error: " + excp.Message, excp);
	                    throw (myExcp);
	                }
	
	                connection.Close();
	            }
	            else
	            {
	                Console.WriteLine("InsertMindpixel: Couldn't connect to database " + database_name);
	                Console.WriteLine(exception_str);
	            }
				
			}
        }

        public static void UpdateConnections(
            string server_name,
            string database_name,
            string user_name,
            string password,
            string mp_table_name,
		    string soundex_table_name)
		{
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";			
						
			string connection_str = 
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
			string Query = "SELECT Hash,Question FROM " + mp_table_name + ";";
			ArrayList mps = RunMySqlCommand(Query, connection_str, 2);
			for (int i = 0; i < mps.Count; i++)
			{
				ArrayList row = (ArrayList)mps[i];
				int hash = Convert.ToInt32(Convert.ToString(row[0]));
				string question = Convert.ToString(row[1]);
				
				UpdateConnections(
				    hash,
				    question,
				    server_name,
				    database_name,
				    user_name,
				    password,
				    mp_table_name,
					soundex_table_name);
				Console.WriteLine(i.ToString());
			}
		}
		
        public static void UpdateConnections(
		    int hash,
            string question,
            string server_name,
            string database_name,
            string user_name,
            string password,
            string mp_table_name,
		    string soundex_table_name)
        {
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";			
						
			string connection_str = 
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
			string Query = "SELECT SoundexHash FROM " + soundex_table_name + " WHERE MindpixelHash = " + hash.ToString() + " GROUP BY SoundexHash;";
			ArrayList soundex_hashes = RunMySqlCommand(Query, connection_str, 1);
			if (soundex_hashes.Count > 0)
			{
				List<int> hashes = new List<int>();
				for (int i = 0; i < soundex_hashes.Count; i++)
				{
					ArrayList row = (ArrayList)soundex_hashes[i];
					string shash = Convert.ToString(row[0]);
			        Query = "SELECT MindpixelHash FROM " + soundex_table_name + " WHERE SoundexHash = " + shash + " GROUP BY MindpixelHash;";
			        ArrayList mindpixel_hashes = RunMySqlCommand(Query, connection_str, 1);
					for (int j = 0; j < mindpixel_hashes.Count; j++)
					{
						ArrayList row2 = (ArrayList)mindpixel_hashes[j];
						string mphash = Convert.ToString(row2[0]);
						int mphash2 = Convert.ToInt32(mphash);
						if (mphash2 != hash)
						{
							if (hashes.IndexOf(mphash2) == -1)
								hashes.Add(mphash2);
						}
					}					
				}
				hashes.Sort();
				string hashes_str = "";
				for (int i = 0; i < hashes.Count; i++)
				{
					hashes_str += hashes[i] + " ";
				}
				hashes_str = hashes_str.Trim();
				float hashes_index = GetNgramIndex(hashes_str.Trim(), 100);
								
				Query = "UPDATE " + mp_table_name + 
						" SET Connections='" + hashes_index.ToString() + "'" +
					    " WHERE Hash=" + hash.ToString() + ";";
					
                MySqlConnection connection = new MySqlConnection();

                connection.ConnectionString = connection_str;

                bool connected = false;
                try
                {
                    connection.Open();
                    connected = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
				    connection.Close();
                }

                if (connected)
                {
		            MySqlCommand update = new MySqlCommand(Query, connection);
		
	                try
	                {
	                    update.ExecuteNonQuery();
	                }
	                catch (Exception excp)
	                {
	                    Exception myExcp = new Exception("Could not update mindpixel. Error: " + excp.Message, excp);
	                    throw (myExcp);
	                }
		
	                connection.Close();					
			    }	
			}
			
        }
				
        public static void InsertWordsIntoMySql(
		    int hash,
            string question,
            string server_name,
            string database_name,
            string user_name,
            string password,
            string table_name,
            List<string> fields_to_be_inserted,
            List<string> field_type)
        {
			
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";

			string connection_str = 
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
			string question_str = RemoveCommonWords(question);
			string[] str2 = question_str.Split(' ');
            List<string> field_name = new List<string>();
            List<string> field_value = new List<string>();
			
			for (int wrd = 0; wrd < str2.Length; wrd++)
			{
				if (str2[wrd].Length > 2)
				{
					//Console.WriteLine(str2[wrd]);
					int word_sound_hash = GetHashCode(Soundex.ToSoundexCode(str2[wrd]));

		            field_name.Clear();
		            field_value.Clear();
					
					field_name.Add("SoundexHash");
					field_value.Add(word_sound_hash.ToString());
					field_name.Add("MindpixelHash");
					field_value.Add(hash.ToString());
					
					// add new pixel
		            MySqlConnection connection = new MySqlConnection();
					
					connection.ConnectionString = connection_str;
			
		            bool connected = false;
		            string exception_str = "";
		            try
		            {
		                connection.Open();
		                connected = true;
		            }
		            catch (Exception ex)
		            {
		                exception_str = ex.Message;
						connection.Close();
		            }
		
		            if (connected)
		            {
		                string Query = "INSERT INTO " + table_name + "(";
		
		                for (int i = 0; i < field_name.Count; i++)
		                {
		                    if ((fields_to_be_inserted.Count == 0) ||
		                        (fields_to_be_inserted.Contains(field_name[i])))
		                    {
		                        Query += field_name[i];
		                        if (i < field_name.Count - 1) Query += ",";
		                    }
		                }
		                Query += ") values(";
		                for (int i = 0; i < field_value.Count; i++)
		                {
		                    int idx = fields_to_be_inserted.IndexOf(field_name[i]);
		
		                    if ((fields_to_be_inserted.Count == 0) ||
		                        (fields_to_be_inserted.Contains(field_name[i])))
		                    {
		                        if (fields_to_be_inserted.Count > 0)
		                        {
		                            if (idx > -1)
		                            {
										if (field_type[idx].Contains("BINARY"))
										{
											field_value[i] = "?Id";
										}
										
		                                if (field_type[idx] == "DATETIME")
		                                {
		                                    DateTime d = DateTime.Parse(field_value[i]);
		                                    string d_str = d.Year.ToString() + "-" + d.Month.ToString() + "-" + d.Day.ToString() + " " + d.Hour.ToString() + ":" + d.Minute.ToString() + ":" + d.Second.ToString();
		                                    field_value[i] = d_str;
		                                }
		                            }
		                        }
		
								if (!field_type[idx].Contains("BINARY"))
		                            Query += "'" + field_value[i] + "'";
								else
									Query += field_value[i];
		                        if (i < field_value.Count - 1) Query += ",";
		                    }
		                }
		                Query += ")";
								
		                MySqlCommand addxml = new MySqlCommand(Query, connection);
		
		                //Console.WriteLine("Running insert query:");
		                //Console.WriteLine(Query);
		
		                try
		                {
		                    addxml.ExecuteNonQuery();
		                }
		                catch (Exception excp)
		                {
		                    Exception myExcp = new Exception("Could not add new mindpixel. Error: " + excp.Message, excp);
		                    throw (myExcp);
		                }
		
		                connection.Close();
		            }
		            else
		            {
		                Console.WriteLine("InsertMindpixel: Couldn't connect to database " + database_name);
		                Console.WriteLine(exception_str);
		            }
				
				}
			}
			
        }		
		
		private static int GCD(int a, int b)
		{
		     while (a != 0 && b != 0)
		     {
		         if (a > b)
		            a %= b;
		         else
		            b %= a;
		     }
		
		     if (a == 0)
		         return b;
		     else
		         return a;
		}		
		
        private static void InsertMindpixelWithCoherence(
		    int hash,
            string question,
            float coherence,
            string server_name,
            string database_name,
            string user_name,
            string password,
            string table_name,
            List<string> fields_to_be_inserted,
            List<string> field_type)
        {
			int plot_type = 0;
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";
			
			string index_ngram3 = phoneme.ToNgramStandardised(question, 3, false);
			string index_soundex = Soundex.ToSoundexStandardised(question, false, false);
			string index_verbs = verbs.GetVerbClasses(question);
			string index_metaphone_primary="", index_metaphone_secondary="";
			Metaphone.ToMetaphoneStandardised(question, false, ref index_metaphone_primary, ref index_metaphone_secondary);
			string index_nysiis = NYSIIS.ToNYSIISStandardised(question, false);

			float coordinate_ngram3 = GetNgramIndex(index_ngram3, 80);
			float coordinate_soundex = GetNgramIndex(index_soundex, 80);
			float coordinate_verbs = GetNgramIndex(index_verbs, 80);
			float coordinate_nysiis = GetNgramIndex(index_nysiis, 80);
			float coordinate_metaphone_primary = GetNgramIndex(index_metaphone_primary, 80);
			float coordinate_metaphone_secondary = GetNgramIndex(index_metaphone_secondary, 80);
			float coordinate_emotion = GetEmotionRating(question);
			
			plot_ctr++;
			if (plot_ctr > 10000)
			{
				float[] coherence2 = null;
				int[] hash1 = null;
				int[] hash2 = null;
				int[] hash3 = null;
				float[] index1 = null;
				float[] index2 = null;
				float[] index3 = null;
                ShowPlot(
                    server_name,
                    database_name,
                    user_name,
                    password,
		            1000,
		            "mindpixels.bmp",
				    false,
				    plot_type,
				    ref coherence2,
				    ref hash1,
				    ref hash2,
				    ref hash3,
				    ref index1,
				    ref index2,
				    ref index3);
				
				plot_ctr = 0;
			}
			
			string connection_str = 
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
            List<string> field_name = new List<string>();
            List<string> field_value = new List<string>();
				
			int YesVotes = 0;
			int NoVotes = 1;
			
			if (coherence > 0)
			{
				YesVotes = (int)(coherence*100);
				NoVotes = 100 - YesVotes;
				int denom = GCD(YesVotes,100);
				if (denom > 0)
				{
				    YesVotes /= denom;
				    NoVotes /= denom;
				}
			}			

			field_name.Add("Id");
			field_value.Add(System.Guid.NewGuid().ToString());
			field_name.Add("Hash");
			field_value.Add(hash.ToString());
			field_name.Add("Question");
			field_value.Add(question);
			field_name.Add("YesVotes");
			field_value.Add(YesVotes.ToString());				
			field_name.Add("NoVotes");
			field_value.Add(NoVotes.ToString());				
			field_name.Add("Coherence");
			field_value.Add(coherence.ToString());
			field_name.Add("Ngram3");
			field_value.Add(coordinate_ngram3.ToString());
			field_name.Add("Soundex");
			field_value.Add(coordinate_soundex.ToString());
			field_name.Add("Verbs");
			field_value.Add(coordinate_verbs.ToString());
		    field_name.Add("Connections");
		    field_value.Add("0");
			field_name.Add("NYSIIS");
			field_value.Add(coordinate_nysiis.ToString());
		    field_name.Add("MetaphonePrimary");
		    field_value.Add(coordinate_metaphone_primary.ToString());
		    field_name.Add("MetaphoneSecondary");
		    field_value.Add(coordinate_metaphone_secondary.ToString());
			field_name.Add("Emotion");
			field_value.Add(coordinate_emotion.ToString());
			
			// add new pixel
            MySqlConnection connection = new MySqlConnection();
			
			connection.ConnectionString = connection_str;
	
            bool connected = false;
            string exception_str = "";
            try
            {
                connection.Open();
                connected = true;
            }
            catch (Exception ex)
            {
                exception_str = ex.Message;
				connection.Close();
            }

            if (connected)
            {
                string Query = "INSERT INTO " + table_name + "(";

                for (int i = 0; i < field_name.Count; i++)
                {
                    if ((fields_to_be_inserted.Count == 0) ||
                        (fields_to_be_inserted.Contains(field_name[i])))
                    {
                        Query += field_name[i];
                        if (i < field_name.Count - 1) Query += ",";
                    }
                }
                Query += ") values(";
                for (int i = 0; i < field_value.Count; i++)
                {
                    int idx = fields_to_be_inserted.IndexOf(field_name[i]);

                    if ((fields_to_be_inserted.Count == 0) ||
                        (fields_to_be_inserted.Contains(field_name[i])))
                    {
                        if (fields_to_be_inserted.Count > 0)
                        {
                            if (idx > -1)
                            {
								if (field_type[idx].Contains("BINARY"))
								{
									field_value[i] = "?Id";
								}
								
                                if (field_type[idx] == "DATETIME")
                                {
                                    DateTime d = DateTime.Parse(field_value[i]);
                                    string d_str = d.Year.ToString() + "-" + d.Month.ToString() + "-" + d.Day.ToString() + " " + d.Hour.ToString() + ":" + d.Minute.ToString() + ":" + d.Second.ToString();
                                    field_value[i] = d_str;
                                }
                            }
                        }

						if (!field_type[idx].Contains("BINARY"))
                            Query += "'" + field_value[i] + "'";
						else
							Query += field_value[i];
                        if (i < field_value.Count - 1) Query += ",";
                    }
                }
                Query += ")";

                MySqlCommand addxml = new MySqlCommand(Query, connection);

                try
                {
					addxml.Parameters.Add("Id", MySqlDbType.Binary).Value = System.Guid.NewGuid().ToByteArray();
                    addxml.ExecuteNonQuery();
                }
                catch
                {
                }

                connection.Close();
            }
            else
            {
                Console.WriteLine("InsertMindpixel: Couldn't connect to database " + database_name);
                Console.WriteLine(exception_str);
            }
        }
		
		/// <summary>
        /// convert probability to log odds
        /// </summary>
        /// <param name="probability"></param>
        /// <returns></returns>
        private static float LogOdds(float probability)
        {
            if (probability > 0.999f) probability = 0.999f;
            if (probability < 0.001f) probability = 0.001f;
            return ((float)Math.Log10(probability / (1.0f - probability)));
        }
		
        /// <summary>
        /// convert a log odds value back into a probability value
        /// </summary>
        /// <param name="logodds"></param>
        /// <returns></returns>
        private static float LogOddsToProbability(float logodds)
        {
            return(1.0f - (1.0f/(1.0f + (float)Math.Exp(logodds))));
        }		
		
        public static float Gaussian(float fraction)
        {
            fraction *= 3.0f;
            float prob = (float)((1.0f / Math.Sqrt(2.0*Math.PI))*Math.Exp(-0.5*fraction*fraction));

            return (prob*2.5f);
        }
				
        private static void ShowPlot(
            string server_name,
            string database_name,
            string user_name,
            string password,
		    int image_width,
		    string filename,
		    bool mono,
		    int plot_type,
		    ref float[] coherence,
		    ref int[] hash1,
		    ref int[] hash2,
		    ref int[] hash3,
		    ref float[] index1,
		    ref float[] index2,
		    ref float[] index3)
        {
			int radius = 2;						
			int depth = 24; //3;
			if (plot_type == 2) 
			{
				depth = 1;
				radius = 3;
			}
			if (plot_type == 3)
			{
				radius=3;
				depth=3;
			}
			int radius_sqr = radius*radius;
			Bitmap bmp = new Bitmap(image_width, image_width, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			byte[] img = new byte[image_width * image_width * 3];

			coherence = new float[image_width * image_width * depth];
			bool[] occupied = new bool[image_width * image_width * depth];
			
			float[] fraction_lookup = new float[(radius*2+1)*(radius*2+1)];
			int fctr = 0;
			for (int i = -radius; i <= radius; i++)
			{
			    for (int j = -radius; j <= radius; j++, fctr++)
			    {
					float dist = (float)Math.Sqrt(i*i + j*j);
				    float fraction = dist / (float)radius;
					fraction_lookup[fctr] = Gaussian(fraction);					
				}
			}
			
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";
			
			string connection_str = 
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
            string Query = "SELECT Hash,YesVotes,NoVotes,Emotion,Ngram3 FROM mindpixels ORDER BY Ngram3;";
			ArrayList result_ngram3 = RunMySqlCommand(Query, connection_str, 5);

			Query = "SELECT Hash,Soundex FROM mindpixels ORDER BY Soundex;";
			ArrayList result_soundex = RunMySqlCommand(Query, connection_str, 2);

			Query = "SELECT Hash,verbs FROM mindpixels ORDER BY verbs;";
			ArrayList result_verbs = RunMySqlCommand(Query, connection_str, 2);
			
			if ((result_ngram3 != null) &&
			    (result_soundex != null) &&
			    (result_verbs != null))
			{
				hash1 = new int[result_ngram3.Count];
				hash2 = new int[result_soundex.Count];
				hash3 = new int[result_verbs.Count];
				index1 = new float[result_ngram3.Count];
				index2 = new float[result_soundex.Count];
				index3 = new float[result_verbs.Count];
				float[] pixelcoherence = new float[result_ngram3.Count];
				float[] pixelemotion = new float[result_ngram3.Count];
				
				int max = result_ngram3.Count;
				for (int i = 0; i < max; i++)
				{
					ArrayList row = (ArrayList)result_ngram3[i];
					string s0 = Convert.ToString(row[0]);
					string s1 = Convert.ToString(row[1]);
					string s2 = Convert.ToString(row[2]);
					string s3 = Convert.ToString(row[3]);
					string s4 = Convert.ToString(row[4]);
					hash1[i] = Convert.ToInt32(s0);
					index1[i] = Convert.ToSingle(s4);
					pixelcoherence[i] = Convert.ToSingle(s1) / (Convert.ToSingle(s1)+Convert.ToSingle(s2));
					pixelemotion[i] = Convert.ToSingle(s3);
				}
				for (int i = 0; i < max; i++)
				{
					ArrayList row = (ArrayList)result_soundex[i];
					string s0 = Convert.ToString(row[0]);
					string s1 = Convert.ToString(row[1]);
					hash2[i] = Convert.ToInt32(s0);
					index2[i] = Convert.ToSingle(s1);
				}
				for (int i = 0; i < max; i++)
				{
					ArrayList row = (ArrayList)result_verbs[i];
					string s0 = Convert.ToString(row[0]);
					string s1 = Convert.ToString(row[1]);
					hash3[i] = Convert.ToInt32(s0);
					index3[i] = Convert.ToSingle(s1);
				}
				for (int i = 0; i < max; i++)
				{				
					int j = Array.IndexOf(hash2, hash1[i]);
					if (j > -1)
					{
						int k = Array.IndexOf(hash3, hash1[i]);
						
						int x = (int)(i * image_width / (float)max);
						int y = (int)(j * image_width / (float)max);
						int z = (int)(k * depth / (float)max);
						
						fctr = 0;
						for (int yy = y - radius; yy <= y + radius; yy++)
						{
							int dy = yy-y;
						    for (int xx = x - radius; xx <= x + radius; xx++, fctr++)
						    {													
								if (((xx > -1) && (xx < image_width)) &&
								    ((yy > -1) && (yy < image_width)))
								{
									int dx = xx-x;
									int r2 = dx*dx + dy*dy;
									if (r2 < radius_sqr)
									{									
								        int n = (((yy * image_width) + xx) * depth) + z;
									    float incr = LogOdds(0.5f + ((pixelcoherence[i]-0.5f) * fraction_lookup[fctr]));
									    coherence[n] += incr;
									    occupied[n] = true;
									}
								}
							}
							
						}
						
					}
					else
					{
						Console.WriteLine(hash1[i].ToString() + " not found");
					}
				}
			}
			else
			{
				Console.WriteLine("No data");				
			}
			
			float prob;
			int n2 = 0;
			
			for (int k = coherence.Length-1; k >= 0; k--)
				coherence[k] = LogOddsToProbability(coherence[k]);
			
			for (int k = 0; k < img.Length; k+=3, n2+=depth)
			{									
				switch (plot_type)
				{
				    case 0:
				    {
					    int bit;
						for (int d = 0; d < depth; d++)
						{
							prob = 0.5f;
							if (occupied[n2+d])
							{
							    prob = coherence[n2+d];	
							}
							
							if (prob > 0.5f)
							{
							    if (d < 8)
							    {
								    bit = (int)Math.Pow(2, d);
								    img[k] = (byte)(((int)img[k]) | bit);
							    }
							    else
							    {
								    if (d < 16)
								    {
								        bit = (int)Math.Pow(2, d-8);
								        img[k+1] = (byte)(((int)img[k+1]) | bit);
								    }
								    else
								    {
								        bit = (int)Math.Pow(2, d-16);
								        img[k+2] = (byte)(((int)img[k+2]) | bit);
								    }
							    }
								//img[k + (d % 3)] = (byte)((prob-0.5f)*255*2);
							}
						    
						}
					    break;
				    }

				    case 3:
				    {
						for (int d = 0; d < depth; d++)
						{
							prob = 0.5f;
							if (occupied[n2+d])
							{
							    prob = coherence[n2+d];	
							}
						    else
						    {
							    int y = (k/3) / image_width;
							    int x = (k/3) % image_width;
							    int search_radius = 6;
							    int search_radius_sqr = search_radius * search_radius;
							    int tx = x - search_radius;
							    if (tx < 0) tx = 0;
							    int ty = y - search_radius;
							    if (ty < 0) ty = 0;
							    int bx = x + search_radius;
							    if (bx >= image_width) bx = image_width-1;
							    int by = y + search_radius;
							    if (by >= image_width) by = image_width-1;
							    float tot = 0;
							    float weight = 0;
							    for (int yy = ty; yy <= by; yy++)
							    {
								    int dy = yy-y;
								    for (int xx = tx; xx <= bx; xx++)
								    {																		    
									    int k2 = (((yy * image_width) + xx) * 3) + d;
									    if (occupied[k2])
									    {
										    int dx = xx-x;
										    float dist_sqr = dx*dx + dy*dy;
										    if (dist_sqr < search_radius_sqr)
										    {
										        float dist_inv = 1.0f / (1.0f + dist_sqr);
										        tot += (coherence[k2]-0.5f) * dist_inv;
										        weight += dist_inv;
										    }
									    }
								    }
							    }
							    if (tot != 0)
							    {
								    prob = (tot / weight) + 0.5f;
							    }
						    }
							
							img[k+d] = (byte)(prob*255);
						}
					    break;
				    }
					
					case 1:
					{
						if ((occupied[n2]) && (occupied[n2+1]))
						{
							if (((coherence[n2] > 0.55f) && (coherence[n2+1] < 0.45f)) ||
							    ((coherence[n2] < 0.45f) && (coherence[n2+1] > 0.55f)))
							    img[k+2] = (byte)255;
						}
						if ((occupied[n2+1]) && (occupied[n2+2]))
						{
							if (((coherence[n2+1] > 0.55f) && (coherence[n2+2] < 0.45f)) ||
							    ((coherence[n2+1] < 0.45f) && (coherence[n2+2] > 0.55f)))
							    img[k+2] = (byte)255;
						}
						break;
					}
								
					case 2:
					{
						if (occupied[n2])
						{
						    prob = coherence[n2];	
							
							if (mono)
							{
								img[k] = (byte)(prob*255);
								img[k+1] = (byte)(prob*255);
								img[k+2] = (byte)(prob*255);
							}
							else
							{					
								if (prob > 0.5f)
								{
									int v = (int)((prob-0.5f) * 255*2);
									if (v > 255) v = 255;
								    img[k+1] = (byte)v;
								}
								else
								{
									int v = -(int)((prob-0.5f) * 255*2);
									if (v > 255) v = 255;
									img[k+2] = (byte)(v);
									img[k] = (byte)(255-v);							
								}					
							}
						}
						break;
					}
				}
				
			}
			
			if ((filename != "") && (filename != null))
			{
				BitmapArrayConversions.updatebitmap_unsafe(img, bmp);
				if (filename.EndsWith("bmp")) bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Bmp);
				if (filename.EndsWith("jpg")) bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
				if (filename.EndsWith("png")) bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
				if (filename.EndsWith("gif")) bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Gif);
			}
        }		
		
        public static void InsertMindpixelUserDataIntoMySql(
		    int hash,
            string question,
            bool answer,
            string server_name,
            string database_name,
            string user_name,
            string password,
            string table_name,
            List<string> fields_to_be_inserted,
            List<string> field_type)
        {
            List<string> field_name = new List<string>();
            List<string> field_value = new List<string>();

			field_name.Add("TimeStamp");
			field_value.Add(DateTime.Now.ToString());
			field_name.Add("Username");
			field_value.Add(user_name);
			field_name.Add("Hash");
			field_value.Add(hash.ToString());
			field_name.Add("Question");
			field_value.Add(question);
			field_name.Add("Answer");
			if (answer == true)
			    field_value.Add("1");
			else
				field_value.Add("0");

            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";
			
            MySqlConnection connection = new MySqlConnection();

            connection.ConnectionString =
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";

            bool connected = false;
            string exception_str = "";
            try
            {
                connection.Open();
                connected = true;
            }
            catch (Exception ex)
            {
                exception_str = ex.Message;
				connection.Close();
            }

            if (connected)
            {
                string Query = "INSERT INTO " + table_name + "(";

                for (int i = 0; i < field_name.Count; i++)
                {
                    if ((fields_to_be_inserted.Count == 0) ||
                        (fields_to_be_inserted.Contains(field_name[i])))
                    {
                        Query += field_name[i];
                        if (i < field_name.Count - 1) Query += ",";
                    }
                }
                Query += ") values(";
                for (int i = 0; i < field_value.Count; i++)
                {
                    int idx = fields_to_be_inserted.IndexOf(field_name[i]);

                    if ((fields_to_be_inserted.Count == 0) ||
                        (fields_to_be_inserted.Contains(field_name[i])))
                    {
                        if (fields_to_be_inserted.Count > 0)
                        {
                            if (idx > -1)
                            {
                                if (field_type[idx] == "DATETIME")
                                {
                                    DateTime d = DateTime.Parse(field_value[i]);
                                    string d_str = d.Year.ToString() + "-" + d.Month.ToString() + "-" + d.Day.ToString() + " " + d.Hour.ToString() + ":" + d.Minute.ToString() + ":" + d.Second.ToString();
                                    field_value[i] = d_str;
                                }
                            }
                        }

                        Query += "'" + field_value[i] + "'";
                        if (i < field_value.Count - 1) Query += ",";
                    }
                }
                Query += ")";

                MySqlCommand addxml = new MySqlCommand(Query, connection);

                //Console.WriteLine("Running query:");
                //Console.WriteLine(Query);

                try
                {
                    addxml.ExecuteNonQuery();
                }
                catch (Exception excp)
                {
                    Exception myExcp = new Exception("Could not add mindpixel. Error: " + excp.Message, excp);
                    throw (myExcp);
                }

                connection.Close();
            }
            else
            {
                Console.WriteLine("Couldn't connect to database " + database_name);
                Console.WriteLine(exception_str);
            }
        }
		
		#endregion
		
		#region "saving the database to a text file"
		
        private static void SaveMindpixels(
		    string save_filename, 
		    string server_name,
		    string database_name, 
		    string user_name, 
		    string password, 
		    string mp_table_name)
		{
            StreamWriter oWrite = null;
            bool allowWrite = true;
            string GAC_str;
            string coherence_str;
            int coherence_int;			
			
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";
			
			int no_of_fields = 6;
			
            string connection_str =
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
		    ArrayList result = RunMySqlCommand(
		        "SELECT * FROM " + mp_table_name + ";", 
		        connection_str, no_of_fields);
			
            try
            {
                oWrite = File.CreateText(save_filename);
            }
            catch
            {
                allowWrite = false;
            }			
			
			if (allowWrite)
			{
				Console.WriteLine("Saving " + result.Count.ToString() + " mindpixels");
				
                oWrite.WriteLine("Mindpixelator Data Set");
                oWrite.WriteLine(result.Count.ToString() + " Propositions with a Corresponding Measure of Human Semantic Coherence");
                oWrite.WriteLine("");
                oWrite.WriteLine(">> Mind Hack with GAC <<");
                oWrite.WriteLine("");
				
				for (int i = 0; i < result.Count; i++)
				{
					ArrayList row = (ArrayList)result[i];

					string question = (string)row[2];
					float coherence = (float)row[5];
							
	                coherence_int = (int)(coherence * 100);
	                coherence_str = Convert.ToString(coherence_int);
					for (int j = 0; j < 3; j++)
					    if (coherence_str.Length < 3) coherence_str = "0" + coherence_str;
	                coherence_str = coherence_str.Substring(0, 1) + "." + coherence_str.Substring(1, 2);
	                GAC_str = Convert.ToString((char)9) + coherence_str + Convert.ToString((char)9) + question;
	                oWrite.WriteLine(GAC_str);
				}
				
	            oWrite.Close();		
				
				Console.WriteLine("Done");				
			}
		}
		
		#endregion

		#region "saving pixels for a particular user"
		
        private static void SaveUserPixels(
		    string save_filename, 
		    string server_name,
		    string database_name, 
		    string user_name, 
		    string password, 
		    string users_table_name)
		{
            StreamWriter oWrite = null;
            bool allowWrite = true;
			
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";
			
			int no_of_fields = 5;
			
            string connection_str =
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
		    ArrayList result = RunMySqlCommand(
		        "SELECT * FROM " + users_table_name + " WHERE Username = '" + user_name + "';", 
		        connection_str, no_of_fields);
			
            try
            {
                oWrite = File.CreateText(save_filename);
            }
            catch
            {
                allowWrite = false;
            }			
			
			if (allowWrite)
			{
				Console.WriteLine("Saving " + result.Count.ToString() + " mindpixels for user " + user_name);
				
				for (int i = 0; i < result.Count; i++)
				{
					ArrayList row = (ArrayList)result[i];

					string question = (string)row[3];
					int answer = Convert.ToInt32(row[4]);
					string answer_str = "Yes";
					if (answer == 0) answer_str = "No";
	                oWrite.WriteLine(answer_str + (char)9 + question);
				}
				
	            oWrite.Close();		
				
				Console.WriteLine("Done");				
			}
		}
		
		#endregion

		#region "return number of records in a table"
		
        private static int NoOfRecords(
		    string server_name,
		    string database_name, 
		    string user_name, 
		    string password, 
		    string table_name)
		{
			int count = 0;
		
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";
			
            string connection_str =
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
		    ArrayList result = RunMySqlCommand(
			    "SELECT COUNT(1) FROM " + table_name + ";",
		        connection_str, 1);
						
			if (result.Count > 0)
			{
				ArrayList row = (ArrayList)result[0];
				count = Convert.ToInt32(row[0]);
			}
			
			return(count);
		}

        private static int NoOfTrueRecords(
		    string server_name,
		    string database_name, 
		    string user_name, 
		    string password, 
		    string table_name)
		{
			int count = 0;
		
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";
			
            string connection_str =
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
		    ArrayList result = RunMySqlCommand(
			    "SELECT COUNT(1) FROM " + table_name + " WHERE YesVotes > NoVotes;",
		        connection_str, 1);
						
			if (result.Count > 0)
			{
				ArrayList row = (ArrayList)result[0];
				count = Convert.ToInt32(row[0]);
			}
			
			return(count);
		}

        private static int NoOfFalseRecords(
		    string server_name,
		    string database_name, 
		    string user_name, 
		    string password, 
		    string table_name)
		{
			int count = 0;
		
            if ((server_name == "") ||
                (server_name == null))
                server_name = "localhost";
			
            string connection_str =
                "server=" + server_name + ";"
                + "database=" + database_name + ";"
                + "uid=" + user_name + ";"
                + "password=" + password + ";";
			
		    ArrayList result = RunMySqlCommand(
			    "SELECT COUNT(1) FROM " + table_name + " WHERE YesVotes < NoVotes;",
		        connection_str, 1);
						
			if (result.Count > 0)
			{
				ArrayList row = (ArrayList)result[0];
				count = Convert.ToInt32(row[0]);
			}
			
			return(count);
		}
		
		#endregion

		#region "saving random mindpixels to a text file"
		
        private static void SaveRandomMindpixels(
		    string save_filename, 
		    string server_name,
		    string database_name, 
		    string user_name, 
		    string password, 
		    string mp_table_name,
		    int no_of_pixels)
		{
			Random rnd = new Random();
			bool probably_true = false;
			if (rnd.Next(10000) > 5000) probably_true = true;
			
			int no_of_records;
			if (probably_true)
			    no_of_records = NoOfTrueRecords(
		            server_name,
		            database_name, 
		            user_name, 
		            password, 
		            mp_table_name);
			else
			    no_of_records = NoOfFalseRecords(
		            server_name,
		            database_name, 
		            user_name, 
		            password, 
		            mp_table_name);
							             
			if (no_of_pixels < 1) no_of_pixels = 1;			
			int max = no_of_records-no_of_pixels+1;
			if ((no_of_records > 0) && (max > 0))
			{			    
			    int start_row = rnd.Next(max);				
				
	            StreamWriter oWrite = null;
	            bool allowWrite = true;
								
	            if ((server_name == "") ||
	                (server_name == null))
	                server_name = "localhost";
				
				int no_of_fields = 6;
				
	            string connection_str =
	                "server=" + server_name + ";"
	                + "database=" + database_name + ";"
	                + "uid=" + user_name + ";"
	                + "password=" + password + ";";
				
				string Query = "";
				if (probably_true)
				    Query = "SELECT * FROM " + mp_table_name + " WHERE YesVotes > NoVotes LIMIT " + no_of_pixels.ToString() + " OFFSET " + start_row.ToString();
				else
				    Query = "SELECT * FROM " + mp_table_name + " WHERE YesVotes < NoVotes LIMIT " + no_of_pixels.ToString() + " OFFSET " + start_row.ToString();
				
			    ArrayList result = RunMySqlCommand(
				    Query,
			        connection_str, no_of_fields);
				
	            try
	            {
	                oWrite = File.CreateText(save_filename);
	            }
	            catch
	            {
	                allowWrite = false;					
	            }			
				
				if (allowWrite)
				{
					Console.WriteLine("Saving " + result.Count.ToString() + " random mindpixels");
					
					for (int i = 0; i < result.Count; i++)
					{
						ArrayList row = (ArrayList)result[i];
	
						string question = (string)row[2];
						float coherence = (float)row[5];
						
		                oWrite.WriteLine(((int)(coherence*100)/100.0f).ToString() + (char)9 + question);
					}
					
		            oWrite.Close();		
					
					Console.WriteLine("Done");				
				}
			}
		}
		
		#endregion
				
		#region "load mindpixels from text file"
		
		static string ToNumeric(string str)
		{
			string result = "";
			char[] ch = str.ToCharArray();
			for (int i = 0; i < ch.Length; i++)
			{
				if (((ch[i] >= '0') && (ch[i] <= '9')) || (ch[i]=='.'))
					result += ch[i];
			}
			return(result);
		}
		
        static void loadGAC(
		    string mindpixels_filename, 
            string initialstring,
		    string[] mp_fields,
		    string[] users_fields,
		    string[] soundex_fields,
		    string server_name,
		    string database_name, 
		    string user_name, 
		    string password, 
		    string mp_table_name,
		    string users_table_name)
		    //string soundex_table_name)
        {
            int no_of_records = NoOfRecords(
		        server_name,
		        database_name, 
		        user_name, 
		        password, 
		        mp_table_name);	
			
			if (no_of_records > 1)
			{
				Console.WriteLine(mp_table_name + " table is not empty.  Please empty the table before loading");
			}
			else
			{	
				no_of_records = NoOfRecords(
		            server_name,
		            database_name, 
		            user_name, 
		            password, 
		            users_table_name);	
				if (no_of_records > 1)
				{
					Console.WriteLine(users_table_name + " table is not empty.  Please empty the table before loading");
				}
				else
				{									
				    //no_of_records = NoOfRecords(
		                //server_name,
		                //database_name, 
		                //user_name, 
		                //password, 
		                //soundex_table_name);
			        //if (no_of_records > 1)
					//{
						//Console.WriteLine(soundex_table_name + " table is not empty.  Please empty the table before loading");
					//}
					//else
					{				
			            bool filefound = true;
			            string str, question;
			            float coherence;
						StreamReader oRead = null;
						Random rnd = new Random();
						
			            List<string> mp_fields_to_be_inserted = new List<string>();
			            List<string> mp_field_type = new List<string>();
			            for (int i = 0; i < mp_fields.Length; i += 2)
			            {
			                mp_fields_to_be_inserted.Add(mp_fields[i]);
			                mp_field_type.Add(mp_fields[i + 1]);
			            }
	
						List<string> soundex_fields_to_be_inserted = new List<string>();
			            List<string> soundex_field_type = new List<string>();
			            for (int i = 0; i < soundex_fields.Length; i += 2)
			            {
			                soundex_fields_to_be_inserted.Add(soundex_fields[i]);
			                soundex_field_type.Add(soundex_fields[i + 1]);
			            }
						
			            try
			            {
			                oRead = File.OpenText(mindpixels_filename);
			            }
			            catch
			            {
			                filefound = false;
			            }
			
			            if (filefound)
			            {
							Console.WriteLine("WARNING: This may take some time...");
			                bool initialstringFound = false;
							int i = 0;
									
				            while ((!oRead.EndOfStream) && (i < 100000))
				            {						
				                str = oRead.ReadLine();
				                if (!initialstringFound)
				                {
				                    /// look for an initial header string after which the data begins
				                    if (str.Contains(initialstring)) initialstringFound = true;
				                }
				                else
				                {
				                    /// read the data
				                    if (str != "")
				                    {
				                        try
				                        {
				                            coherence = Convert.ToSingle(ToNumeric(str.Substring(1, 4)));
											if (coherence > 1) coherence = 1;
				                            question = str.Substring(6);
											int question_hash = GetHashCode(question);
											
											i++;
											if (rnd.Next(2000) < 2) Console.WriteLine(i.ToString() + (char)9 + question);
											
									        //InsertWordsIntoMySql(
											    //question_hash,
									            //question,
									            //server_name,
									            //database_name,
									            //user_name,
									            //password,
									            //soundex_table_name,
									            //soundex_fields_to_be_inserted,
									            //soundex_field_type);
											
								            InsertMindpixelWithCoherence(
											    question_hash,
								                question,
								                coherence,
								                server_name,
								                database_name,
								                user_name,
								                password,
								                mp_table_name,
								                mp_fields_to_be_inserted,
								                mp_field_type);								
			
				                        }
				                        catch //(Exception ex)
				                        {
											//Console.WriteLine("str: " + str);
											//Console.WriteLine("error: " + ex.Message);
				                        }
				                    }
				                }
				            }
				            if (oRead.EndOfStream)
				            {
				                oRead.Close();
				            }
							
		                    no_of_records = NoOfRecords(
				                server_name,
				                database_name, 
				                user_name, 
				                password, 
				                mp_table_name);	
							
							Console.WriteLine(no_of_records.ToString() + " records loaded");
						}
					}
				}
			}
        }

	    static string[] common_words = {
	        "a",
	        "an",
	        "am",
	        "at",
	        "to",
	        "as",
	        "we",
	        "i",
	        "in",
	        "is",
	        "it",
	        "if",
	        "be",
	        "by",
	        "so",
			"than",
			"does",
			"did",
	        "no",
	        "last",
	        "first",
	        "on",
	        "of",
	        "its",
	        "all",
	        "can",
	        "into",
	        "from",
	        "just",
	        "and",
	        "the",
	        "over",
	        "under",
	        "for",
	        "then",
	        "dont",
	        "has",
	        "get",
	        "got",
	        "had",
	        "should",
	        "hadnt",
	        "have",
	        "some",
	        "come",
	        "this",
	        "call",
	        "that",
	        "thats",
	        "find",
	        "these",
	        "them",
	        "look",
	        "looked",
	        "looks",
	        "with",
	        "but",
	        "about",
	        "where",
	        "possible",
	        "sometimes",
	        "which",
	        "they",
	        "just",
	        "we",
	        "while",
	        "whilst",
	        "their",
	        "perhaps",
	        "you",
	        "make",
	        "any",
	        "say",
	        "been",
	        "like",
	        "form",
	        "our",
	        "give",
	        "in the",
	        "in a",
	        "will",
	        "object",
	        "shall",
	        "will not",
	        "until",
	        "take",
	        "other",
	        "now",
	        "lead",
	        "taken",
	        "you can",
	        "have to",
	        "have some",
	        "would",
	        "said",
	        "one",
	        "how",
	        "new",
	        "we",
	        "said",
	        "it",
	        "was",
	        "are",
	        "every",
	        "such",
	        "more",
	        "different",
	        "example",
	        "way",
	        "only",
	        "often",
	        "show",
	        "group",
	        "itself",
	        "part",
	        "saw",
	        "making",
	        "could",
	        "need",
	        "out",
	        "being",
	        "been",
	        "yet",
	        "lack",
	        "even",
	        "own",
	        "much",
	        "of this",
	        "become",
	        "keep",
	        "keeps",
	        "do",
	        "having",
	        "normal",
	        "this",
	        "after",
	        "before",
	        "during",
	        "off",
	        "use",
	        "same",
	        "case",
	        "there",
	        "through",
	        "end",
	        "may",
	        "made",
	        "name",
	        "most",
	        "many",
	        "well",
	        "who",
	        "is",
	        "your",
	        "you",
	        "owner",
	        "around",
	        "about",
	        "of",
	        "process",
	        "too",
	        "my",
	        "why",
	        "tell",
	        "he",
	        "she",
	        "what",
	        "left",
	        "him",
	        "her",
	        "ever",
	        "there"
	    };
			
		private static string TextOnly(
		    string text)
		{
			string result = "";
			char[] ch = text.ToCharArray();
			for (int i = 0; i < text.Length; i++)
			{
				if (((ch[i] >= 'a') &&
					(ch[i] <= 'z')) ||
					((ch[i] >= 'A') &&
					(ch[i] <= 'Z')) ||
	                ((ch[i] >= '0') &&
					(ch[i] <= '9')) ||
	                (ch[i] == ' '))
				result += ch[i];
			}
			return(result);
		}
			
	    private static string RemoveCommonWords(
	        string text)
	    {
			string result = "";
			text = TextOnly(text);
			text = text.ToLower();
				
			string[] str = text.Split(' ');
			
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] != "")
				{
					if (Array.IndexOf(common_words, str[i]) == -1)
						result += str[i] + " ";
				}
			}
				
			return(result.Trim());
		}
		
		static void UpdateWordStatistics(
		    string question, 
		    float coherence,
		    List<string>[] word, 
		    List<int>[] word_frequency, 
		    List<string>[] word_cooccurrence, 
		    List<int>[] word_cooccurrence_frequency,
		    List<float>[] word_cooccurrence_coherence)
		{
		    //question = RemoveCommonWords(question.ToLower());
			question = TextOnly(question.ToLower());
			string[] str = question.Split(' ');
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i].Length > 1)
				{
					char[] ch = str[i].ToCharArray();
					int word_index0 = ch[0];
					int word_index1 = ch[1];
					if ((ch[0] >= 'a') && (ch[0] <= 'z'))
						word_index0 -= (int)'a';
					else
						word_index0 -= (int)'0' + 26;
					if ((ch[1] >= 'a') && (ch[1] <= 'z'))
						word_index1 -= (int)'a';
					else
						word_index1 -= (int)'0' + 26;
					//Console.WriteLine(word_index0.ToString());
					//Console.WriteLine(word_index1.ToString());
					int word_index = word_index0*36+word_index1;
					
					//Console.WriteLine(str[i]);
					int pos = word[word_index].IndexOf(str[i]);
					if (pos == -1)
					{
						word[word_index].Add(str[i]);
						word_frequency[word_index].Add((int)1);
					}
					else
					{
						word_frequency[word_index][pos]++;
					}
					
					for (int j = i+1; j < str.Length; j++)
					{
						if (str[j].Length > 1)
						{
							char[] ch2 = str[j].ToCharArray();
							int word_index2 = ch2[0];
							if ((ch2[0] >= 'a') && (ch2[0] <= 'z'))
								word_index2 -= (int)'a';
							else
								word_index2 -= (int)'0' + 26;
							int word_index3 = ch2[1];
							if ((ch2[1] >= 'a') && (ch2[1] <= 'z'))
								word_index3 -= (int)'a';
							else
								word_index3 -= (int)'0' + 26;
							int word_index_co = (word_index0*36*36*36)+(word_index1*36*36)+(word_index2*36)+word_index3;
							
							string costr = "";
							if (j > i+1) 
							{
								costr = str[i]+"_#_"+str[j];
							}
							else
							{
								costr = str[i] + "_" + str[j];
							}
								
							pos = word_cooccurrence[word_index_co].IndexOf(costr);
							if (pos == -1)
							{
								word_cooccurrence[word_index_co].Add(costr);
								word_cooccurrence_frequency[word_index_co].Add((int)1);
								word_cooccurrence_coherence[word_index_co].Add(LogOdds(0.0f));
							}
							else
							{
								word_cooccurrence_frequency[word_index_co][pos]++;
								word_cooccurrence_coherence[word_index_co][pos]+= LogOdds(coherence);
							}
						}
					}
					
				}
			}
		}
		
        static void loadGACWordFrequencies(
            string server_name,
            string database_name,
            string user_name,
            string password,
		    string mindpixels_filename, 
            string initialstring,
		    List<string> word,
		    List<string> word_cooccurrence)
        {
            bool filefound = true;
            string str, question;
            float coherence;
			StreamReader oRead = null;
			Random rnd = new Random();
			
			int index_max = 36*36;
			int index_max2 = 36*36*36*36;
		    List<string>[] iword = new List<string>[index_max];
			List<int>[] iword_frequency = new List<int>[index_max];
		    List<string>[] iword_cooccurrence = new List<string>[index_max2];
			List<int>[] iword_cooccurrence_frequency = new List<int>[index_max2];
			List<float>[] iword_cooccurrence_coherence = new List<float>[index_max2];
			for (int n = 0; n < index_max; n++)
			{
				iword[n] = new List<string>();
				iword_frequency[n] = new List<int>();
			}
			for (int n = 0; n < index_max2; n++)
			{
				iword_cooccurrence[n] = new List<string>();
				iword_cooccurrence_frequency[n] = new List<int>();
				iword_cooccurrence_coherence[n] = new List<float>();
			}
			
            try
            {
                oRead = File.OpenText(mindpixels_filename);
            }
            catch
            {
                filefound = false;
            }

            if (filefound)
            {
				Console.WriteLine("WARNING: This may take some time...");
                bool initialstringFound = false;
				int i = 0;
						
	            while ((!oRead.EndOfStream) && (i <= 80000))
	            {						
	                str = oRead.ReadLine();
	                if (!initialstringFound)
	                {
	                    /// look for an initial header string after which the data begins
	                    if (str.Contains(initialstring)) initialstringFound = true;
	                }
	                else
	                {
	                    /// read the data
	                    if (str != "")
	                    {
	                        try
	                        {
	                            coherence = Convert.ToSingle(ToNumeric(str.Substring(1, 4)));
								if (coherence > 1) coherence = 1;
	                            question = str.Substring(6);
								
								UpdateWordStatistics(question, coherence, iword, iword_frequency, iword_cooccurrence, iword_cooccurrence_frequency, iword_cooccurrence_coherence);
								if (i % 1000 == 0) Console.WriteLine(i.ToString());
								i++;
	                        }
	                        catch //(Exception ex)
	                        {
								//Console.WriteLine("str: " + str);
								//Console.WriteLine("error: " + ex.Message);
	                        }
	                    }
	                }
	            }
	            if (oRead.EndOfStream)
	            {
	                oRead.Close();
	            }						
			}

			Console.WriteLine("Updating words");
			int max_frequency = 0;
			for (int i = 0; i < index_max; i++)
			{								
				for (int j = 0; j < iword[i].Count; j++)
				{
					string s = iword_frequency[i][j].ToString();
					while (s.Length < 6) s = "0" + s;
					s += " " + iword[i][j];
					word.Add(s);
				}
			}
			word.Sort();
			word.Reverse();
			
			Console.WriteLine("Updating cooccurrence");

			float max_frequency2 = 0.0001f;
			for (int i = 0; i < index_max2; i++)
			{
				for (int j = 0; j < iword_cooccurrence[i].Count; j++)
				{
					if (iword_cooccurrence_frequency[i][j] > max_frequency2)
						max_frequency2 = iword_cooccurrence_frequency[i][j];
				}
			}
			
			for (int i = 0; i < index_max2; i++)
			{
				for (int j = 0; j < iword_cooccurrence[i].Count; j++)
				{
					string s = ((int)(iword_cooccurrence_frequency[i][j]*1000/max_frequency2)/1000.0f).ToString();
					if (s.Length==1) s += ".";
					while (s.Length < 5) s += "0";					
					float coh = (int)(LogOddsToProbability(iword_cooccurrence_coherence[i][j])*1000)/1000.0f;
					string coherence_str = Convert.ToString(coh);
					if (coherence_str.Length == 1) coherence_str += ".";
					while (coherence_str.Length < 5) coherence_str += "0";
					s += " " + coherence_str;
					s += " " + iword_cooccurrence[i][j];
					word_cooccurrence.Add(s);
				}
			}
			word_cooccurrence.Sort();
			word_cooccurrence.Reverse();
			
			Console.WriteLine("Saving");
			StreamWriter oWrite = null;
			filefound = true;			
            try
            {
                oWrite = File.CreateText("mindpixels_word_frequencies.txt");
            }
            catch
            {
                filefound = false;
            }
			
			if (filefound)
			{
				for (int i = 0; i < word.Count; i++)
				{
					oWrite.WriteLine(word[i]);
				}
				oWrite.Close();
			}
			
			filefound = true;			
            try
            {
                oWrite = File.CreateText("mindpixels_word_cooccurrence_frequencies.txt");
            }
            catch
            {
                filefound = false;
            }
			
			if (filefound)
			{
				for (int i = 0; i < word_cooccurrence.Count; i++)
				{
					oWrite.WriteLine(word_cooccurrence[i]);
				}
				oWrite.Close();
			}

        }
				
		#endregion
		
		#region "semantic differentials"
		
		static string[] semantic_dimensions = {
			"good-bad","E",
			"optimistic-pessimistic","E",
			"positive-negative","E",
			"complete-incomplete", "E",
			"timely-untimely","E",			
			"acid-base","E",
			"alive-dead","E",
			"proponent-antagonist","E",
			"front-back","E",
			"white-black","E",
			"tactful-blunt","E",
			"ceiling-floor","E",
			"real-character","E",
			"expensive-cheap","E",
			"concentration-meditation","E",
			"life-death","E",
			"increase-decrease","E",
			"disorder-order","E",
			"diurnal-nocturnal","E",
			"east-west","E",
			"everything-nothing","E",
			"good-evil","E",
			"truth-falsehood","E",
			"false-true","E",
			"truth-falsity","E",
			"near-far","E",
			"fideism-rationalism","E",
			"fission-fusion","E",
			"flammable-inflammable","E",
			"flexible-rigid","E",
			"greater-less","E",
			"happy-sad","E",
			"love-hate","E",
			"truth-lie","E",
			"light-shaddow","E",
			"magic-science","E",
			"man-woman","E",
			"misnight-noon","E",
			"nord-south","E",
			"northern-seasons","E",
			"north-south","E",
			"objective-subjective","E",
			"peace-war","E",
			"permanent-temporary","E",
			"phallic-vaginal","E",
			"recycle-throw","E",
			"right-wrong","E",
			"salty-sweet","E",

			"hard-soft","P",
			"heavy-light","P",
			"feminine-masculine","P",
			"female-male","P",
			"severe-lenient","P",
			"strong-weak","P",
			"tenacious-yielding","P",
			"free-constrained","P",
			"spacious-constricted","P",
			"serious-humourous","P",
			"serious-humorous","P",
			"big-small","P",
			"tall-short","P",
			"transparent-opaque","P",
			"kind-cruel","P",
			"clean-dirty","P",
			"light-dark","P",
			"graceful-awkward","P",
			"pleasurable-painful","P",
			"pleasure-pain","P",
			"beautiful-ugly","P",
			"fast-slow","P",
			"rounded-angular","P",
			"young-old","P",
			"new-old","P",
			"dry-wet","P",
			
			"active-passive","A",
			"cold-hot","A",
			"cold-warm","A",
			"exciting-boring","A",
			"intentional-unintentional","A",
			"complex-simple","A",
			"successful-unsuccessful","A",
			"meaningful-meaningless","A",
			"important-unimportant","A",
			"important-trivial","A",
			"progressive-regressive","A",
			"forward-backwards","A",
		};
		
		static void SemanticDifferentialDimension(
		    string word,
		    ref int Evaluation,
		    ref int Potency,
		    ref int Activity)
		{
			Evaluation=0;
			Potency=0;
			Activity=0;
			
			for (int i = 0; i < semantic_dimensions.Length;i+=2)
			{
				int pos = semantic_dimensions[i].IndexOf(word);
				if (pos > -1)
				{					
					int v = 7;
					if (pos > 1) v = 1;
					if (semantic_dimensions[i+1] == "E") Evaluation = v;
					if (semantic_dimensions[i+1] == "P") Potency = v;
					if (semantic_dimensions[i+1] == "A") Activity = v;
					break;
				}
			}
		}

        static void loadGACWordSpace(
            string server_name,
            string database_name,
            string user_name,
            string password,
		    string mindpixels_filename, 
            string initialstring,
		    List<string> differential,
		    List<string> semantic_differential)
        {
            bool filefound = true;
            string str, question;
            float coherence;
			float[] max_value = new float[differential.Count];
			float[] min_value = new float[differential.Count];
			StreamReader oRead = null;
			
			int index_max = 36*36;
		    List<string>[] iword = new List<string>[index_max];
			List<float[]>[] iword_space = new List<float[]>[index_max];
			
            try
            {
                oRead = File.OpenText(mindpixels_filename);
            }
            catch
            {
                filefound = false;
            }

            if (filefound)
            {
				Console.WriteLine("WARNING: This may take some time...");
                bool initialstringFound = false;
				int i = 0;
						
	            while ((!oRead.EndOfStream) && (i <= 80000))
	            {						
	                str = oRead.ReadLine();
	                if (!initialstringFound)
	                {
	                    /// look for an initial header string after which the data begins
	                    if (str.Contains(initialstring)) initialstringFound = true;
	                }
	                else
	                {
	                    /// read the data
	                    if (str != "")
	                    {
	                        try
	                        {
	                            coherence = Convert.ToSingle(ToNumeric(str.Substring(1, 4)));
								if (coherence > 1) coherence = 1;
	                            question = str.Substring(6);
								
								string question2="";
								int[] word_index = null;
								string[] question_word = null;
								for (int j = differential.Count-1; j >= 0; j--)
								{
									if (question.Contains(" " + differential[j] + " "))
									{
										if (question_word == null)
										{
											question2 = RemoveCommonWords(question);
											question_word = question2.Split(' ');
											
											word_index = new int[question_word.Length];
											for (int ii = 0; ii < question_word.Length; ii++)
											{
												if (question_word[ii].Length > 2)
												{
													if (question_word[ii].EndsWith("s"))
														if (!question_word[ii].EndsWith("ss"))
														    question_word[ii] = question_word[ii].Substring(0, question_word[ii].Length-1);
												}
												
												char[] ch = question_word[ii].ToCharArray();												
												int word_index0 = ch[0];
												int word_index1 = ch[1];
												if ((ch[0] >= 'a') && (ch[0] <= 'z'))
													word_index0 -= (int)'a';
												else
													word_index0 = (word_index0 - (int)'0') + 26;
												if ((ch[1] >= 'a') && (ch[1] <= 'z'))
													word_index1 -= (int)'a';
												else
													word_index1 = (word_index1 - (int)'0') + 26;
												word_index[ii] = word_index0*36+word_index1;
											}
											
										}
										for (int k = 0; k < question_word.Length; k++)
										{
											if (question_word[k] != differential[j])
											{
												int idx = word_index[k];
												if (iword[idx] == null)
													iword[idx] = new List<string>();
												
												if (iword_space[idx] == null)
												{
													iword_space[idx] = new List<float[]>();
												}
											
												int pos2 = iword[idx].IndexOf(question_word[k]);
												if (pos2 == -1)
												{
													//Console.WriteLine("word: " + question_word[k]);
													iword[idx].Add(question_word[k]);
													iword_space[idx].Add(new float[differential.Count]);
													pos2 = 0;
												}
												if (coherence > 0.5f)
												    iword_space[idx][pos2][j] += 1;
												else
													iword_space[idx][pos2][j] -= 1;
												if (iword_space[idx][pos2][j] > max_value[j]) max_value[j] = iword_space[idx][pos2][j];
												if (iword_space[idx][pos2][j] < min_value[j]) min_value[j] = iword_space[idx][pos2][j];
											}
										}
									}
								}
								
								if (i % 1000 == 0) Console.WriteLine(i.ToString());
								i++;
	                        }
	                        catch //(Exception ex)
	                        {
								//Console.WriteLine("str: " + str);
								//Console.WriteLine("error: " + ex.Message);
	                        }
	                    }
	                }
	            }
	            if (oRead.EndOfStream)
	            {
	                oRead.Close();
	            }						
			}

			Console.WriteLine("Updating words");
			for (int i = 0; i < index_max; i++)
			{
				if (iword[i] != null)
				{
				    for (int j = 0; j < iword[i].Count; j++)
					{
						string wrd = iword[i][j];
						char[] ch = wrd.ToCharArray();
						if (!((ch[0] >= '0') && (ch[0] <= '9')))
						{						
							int ctr=0;
							for (int k = 0; k < differential.Count; k++)
								if (iword_space[i][j][k] > 0) ctr++;
							if (ctr > 0)
							{
								Console.Write(wrd + " | ");
								Console.Write(" " + ctr.ToString() + " | ");
								
								ctr=0;
								float[] word_vector = new float[3];
							    int Evaluation=0;
							    int Potency=0;
							    int Activity=0;
								int hits=0;
								for (int k = 0; k < differential.Count; k++)
								{
									float maxval = max_value[k];
									if (-min_value[k] > max_value[k]) maxval = -min_value[k];
									if (iword_space[i][j][k] > 0)
									{
									    float val = (int)(iword_space[i][j][k] * 1000 / maxval)/1000.0f;
									    SemanticDifferentialDimension(differential[k], ref Evaluation, ref Potency, ref Activity);
										word_vector[0] += Evaluation*val;
										word_vector[1] += Potency*val;
										word_vector[2] += Activity*val;
										hits+=7;
									}					
								}
								if (hits>0)
								{
								    word_vector[0] /= hits;
									word_vector[1] /= hits;
									word_vector[2] /= hits;
								}
								Console.WriteLine(" " + word_vector[0].ToString() + " " + word_vector[1].ToString() + " " + word_vector[2].ToString());
								//Console.WriteLine("");
							}
						}
					}
				}
			}
			
			Console.WriteLine("Saving");
			StreamWriter oWrite = null;
			filefound = true;			
            try
            {
                oWrite = File.CreateText("mindpixels_word_space.txt");
            }
            catch
            {
                filefound = false;
            }
			
			if (filefound)
			{
				//for (int i = 0; i < word.Count; i++)
				{
				//	oWrite.WriteLine(word[i]);
				}
				oWrite.Close();
			}

        }
				
        static void loadGACSemanticDifferentials(
            string server_name,
            string database_name,
            string user_name,
            string password,
		    string mindpixels_filename, 
            string initialstring,
		    List<string> polar_term,
		    List<int> polar_term_frequency,
		    List<string> semantic_differential)
        {
            bool filefound = true;
            string str, question;
            float coherence;
			StreamReader oRead = null;			
			int max = 0;
			
            try
            {
                oRead = File.OpenText(mindpixels_filename);
            }
            catch
            {
                filefound = false;
            }

            if (filefound)
            {
				Console.WriteLine("WARNING: This may take some time...");
                bool initialstringFound = false;
				int i = 0;
								
	            while (!oRead.EndOfStream)
	            {						
	                str = oRead.ReadLine();
	                if (!initialstringFound)
	                {
	                    /// look for an initial header string after which the data begins
	                    if (str.Contains(initialstring)) initialstringFound = true;
	                }
	                else
	                {
	                    /// read the data
	                    if (str != "")
	                    {
	                        try
	                        {
	                            coherence = Convert.ToSingle(ToNumeric(str.Substring(1, 4)));
								if (coherence > 1) coherence = 1;
	                            question = str.Substring(6);

								if (coherence > 0.5f)
								{
									if (question.Contains(" opposite of "))
									{
										string qstr = " a type of ";
										int pos2 = question.IndexOf(qstr);
										if (pos2 > -1)
										    question = question.Substring(0,pos2) + " " + question.Substring(pos2 + qstr.Length);

										qstr = " considered ";
										pos2 = question.IndexOf(qstr);
										if (pos2 > -1)
										    question = question.Substring(0,pos2) + " " + question.Substring(pos2 + qstr.Length);

										qstr = " capable of ";
										pos2 = question.IndexOf(qstr);
										if (pos2 > -1)
										    question = question.Substring(0,pos2) + " " + question.Substring(pos2 + qstr.Length);

										qstr = " mean ";
										pos2 = question.IndexOf(qstr);
										if (pos2 > -1)
										    question = question.Substring(0,pos2) + " " + question.Substring(pos2 + qstr.Length);
											
										string wrd0 = "";
										string wrd1 = "";
										string[] str2 = (RemoveCommonWords(question)).Split(' ');
										if ((str2[0] == "opposite") && (str2.Length==3))
										{
											wrd0 = str2[1];
											wrd1 = str2[2];
											//if (wrd0 == "type") Console.WriteLine(question);
										}
										else
										{
											for (int j = 1; j < str2.Length-1; j++)
											{
												if (str2[j] == "opposite")
												{
											        wrd0 = str2[j - 1];
											        wrd1 = str2[j + 1];												
													//if (wrd0 == "type") Console.WriteLine(question);
												}
											}
										}
										if (wrd0 != wrd1)
										{
											if ((wrd0.Length > 2) && (wrd1.Length > 2))
											{
												for (int ii = 0; ii < 2; ii++)
												{
													string diff = wrd0;
													if (ii==1) diff = wrd1;
													
													int pos = polar_term.IndexOf(diff);
													if (pos == -1)
													{													
														polar_term.Add(diff);
														polar_term_frequency.Add(1);
														if (max == 0) max = 1;
													}
													else
													{
														polar_term_frequency[pos]++;
														if (polar_term_frequency[pos] > max) 
															max = polar_term_frequency[pos];
													}
												}
												
												List<string> wrd_list = new List<string>();
												wrd_list.Add(wrd0);
												wrd_list.Add(wrd1);
												wrd_list.Sort();
												string wrdstr = wrd_list[0] + "-" + wrd_list[1];
												pos2 = semantic_differential.IndexOf(wrdstr);
												if (pos2 == -1)
												{
													semantic_differential.Add(wrdstr);
											        Console.WriteLine(wrdstr);
												}
											}
										}
									}
								}
								
								if ((question.Contains(" very ")) ||
								    (question.Contains(" extremely ")) ||
								    (question.Contains(" quite ")) ||
								    (question.Contains(" slightly "))
								    )
								{
									string[] str2 = (TextOnly(question)).Split(' ');
									for (int j = 0; j < str2.Length; j++)
									{
										if ((str2[j] == "very") ||
										    (str2[j] == "extremely") ||
										    (str2[j] == "quite") ||
										    (str2[j] == "slightly"))
										{
											if (j < str2.Length-1)
											{
												string diff = str2[j+1].ToLower();
												if ((diff != "very") &&
												    (diff != "extremely") &&
												    (diff != "quite") &&
												    (diff != "slightly"))
												{
													int pos = polar_term.IndexOf(diff);
													if (pos == -1)
													{													
														polar_term.Add(diff);
														polar_term_frequency.Add(1);
														if (max == 0) max = 1;
													}
													else
													{
														polar_term_frequency[pos]++;
														if (polar_term_frequency[pos] > max) 
															max = polar_term_frequency[pos];
													}
												}
											}
										}
									}
								}
								
								i++;
	                        }
	                        catch //(Exception ex)
	                        {
								//Console.WriteLine("str: " + str);
								//Console.WriteLine("error: " + ex.Message);
	                        }
	                    }
	                }
	            }
	            if (oRead.EndOfStream)
	            {
	                oRead.Close();
	            }						
			}
			
			StreamWriter oWrite = null;
			filefound = true;			
            try
            {
                oWrite = File.CreateText("mindpixels_polar_terms.txt");
            }
            catch
            {
                filefound = false;
            }
			
			if (filefound)
			{
				Console.WriteLine(polar_term.Count.ToString() + " scalar words detected");
				List<string> polar_term_new = new List<string>();
				for (int i = 0; i < polar_term.Count; i++)
				{
					string freq_str = Convert.ToString((int)(polar_term_frequency[i]*100 / (float)max)/100.0f);
					if (freq_str.Length == 1) freq_str += ".";
					while (freq_str.Length < 4) freq_str += "0";
					polar_term_new.Add(freq_str + " " + polar_term[i]);
				}
				polar_term_new.Sort();
				polar_term_new.Reverse();
				polar_term.Clear();
				for (int i = 0; i <polar_term_new.Count; i++)
				{
					oWrite.WriteLine(polar_term_new[i]);
					string[] str2 = polar_term_new[i].Split(' ');
					polar_term.Add(str2[1]);
				}				
				oWrite.Close();
			}
			
			oWrite = null;
			filefound = true;			
            try
            {
                oWrite = File.CreateText("mindpixels_semantic_differentials.txt");
            }
            catch
            {
                filefound = false;
            }
			
			if (filefound)
			{
				Console.WriteLine(semantic_differential.Count.ToString() + " semantic differentials detected");
				semantic_differential.Sort();
				for (int i = 0; i < semantic_differential.Count; i++)
				{
					oWrite.WriteLine(semantic_differential[i]);
				}
				oWrite.Close();
			}
			
			
            loadGACWordSpace(
                server_name,
                database_name,
                user_name,
                password,
		        mindpixels_filename, 
                initialstring,
		        polar_term,
			    semantic_differential);
        }
		
		
		#endregion
		
        #region "validation"

        /// <summary>
        /// returns a list of valid parameter names
        /// </summary>
        /// <returns>list of valid parameter names</returns>
        private static ArrayList GetValidParameters()
        {
            ArrayList ValidParameters = new ArrayList();

            ValidParameters.Add("q");
            ValidParameters.Add("a");
            ValidParameters.Add("username");
            ValidParameters.Add("password");
			ValidParameters.Add("server");
			ValidParameters.Add("wp");
			ValidParameters.Add("fb");
			ValidParameters.Add("db");
			ValidParameters.Add("cn");
			ValidParameters.Add("load");
			ValidParameters.Add("save");
			ValidParameters.Add("map");
			ValidParameters.Add("plot");
			ValidParameters.Add("mapmono");
			ValidParameters.Add("lookup");
			ValidParameters.Add("random");
			ValidParameters.Add("userpixels");
            ValidParameters.Add("help");
            return (ValidParameters);
        }

        #endregion
		
        #region "help information"

        /// <summary>
        /// shows help information
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("mpcreate Help");
            Console.WriteLine("-------------");
            Console.WriteLine("");
            Console.WriteLine("Syntax:  mpcreate");
            Console.WriteLine("");
            Console.WriteLine("         -q <question>");
            Console.WriteLine("         -a <answer>");
            Console.WriteLine("         -username <username>");
            Console.WriteLine("         -password <password>");
            Console.WriteLine("         -server <server name>");
            Console.WriteLine("         -wp <wikipedia schools edition directory>");
            Console.WriteLine("         -fb <Freebase TSV data directory>");
			Console.WriteLine("         -cn <conceptnet RDF file>");
			Console.WriteLine("         -db <mysql database name>");
            Console.WriteLine("         -load <mindpixel file>");
            Console.WriteLine("         -save <mindpixel file>");
            Console.WriteLine("         -map <mindpixel map image file>");
            Console.WriteLine("         -plot <plot type 0-2>");
            Console.WriteLine("         -mapmono <mindpixel map image file>");
            Console.WriteLine("         -lookup <lookup tables file>");
            Console.WriteLine("         -random <filename>");
            Console.WriteLine("         -userpixels <filename>");
            Console.WriteLine("");
            Console.WriteLine("Example: mpcreate.exe -q " + '"' + "Is water wet?" + '"' + " -a yes");
        }

        #endregion	
	}
}